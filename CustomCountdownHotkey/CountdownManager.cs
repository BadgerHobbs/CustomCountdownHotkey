using Gma.System.MouseKeyHook;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows;

namespace CustomCountdownHotkey
{
    public class CountdownManager
    {
        private static JObject ConvertJsonStringToObject(string jsonString)
        {
            // Create json object from string data
            JObject jsonObject = JObject.Parse(jsonString);

            // Return new json object
            return jsonObject;
        }

        private void GenerateDefaultConfigFile()
        {
            dynamic config = new JObject();
            config.HotKeys = new JArray();

            dynamic hotkey = new JObject();
            hotkey.HotKey = "0";
            hotkey.StartTimeSeconds = "30";
            ((JArray)config["HotKeys"]).Add(hotkey);

            string updatedConfigJson = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText("config.json", updatedConfigJson);
        }

        JObject config;

        public void LoadConfigFile()
        {
            if (!File.Exists("config.json"))
            {
                GenerateDefaultConfigFile();
            }

            string configJson = File.ReadAllText("config.json");
            config = ConvertJsonStringToObject(configJson);
        }

        private IKeyboardMouseEvents m_GlobalHook;

        public void Subscribe()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.KeyPress += GlobalHookKeyPress;
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            foreach (JObject hotKey in config["HotKeys"])
            {
                if (e.KeyChar.ToString().ToLower() == hotKey["HotKey"].Value<string>().ToLower())
                {
                    if (timer != null)
                    {
                        timer.Stop();
                        timer = null;
                    }

                    StartCountDown(timerLabel, Convert.ToInt32(hotKey["StartTimeSeconds"].Value<string>()));
                }
            }    
        }

        DispatcherTimer timer;
        TimeSpan time;

        public System.Windows.Controls.Label timerLabel;

        private void StartCountDown(System.Windows.Controls.Label timerLabel, int seconds)
        {
            time = TimeSpan.FromSeconds(seconds);

            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                timerLabel.Content = time.ToString("c");
                if (time == TimeSpan.Zero) timer.Stop();
                time = time.Add(TimeSpan.FromSeconds(-1));
            }, System.Windows.Application.Current.Dispatcher);

            timer.Start();
        }
    }
}

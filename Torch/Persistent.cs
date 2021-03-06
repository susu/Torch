﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Torch
{
    /// <summary>
    /// Simple class that manages saving <see cref="Persistent{T}.Data"/> to disk using XML serialization.
    /// Can automatically save on changes by implementing <see cref="INotifyPropertyChanged"/> in the data class.
    /// </summary>
    /// <typeparam name="T">Data class type</typeparam>
    public sealed class Persistent<T> : IDisposable where T : new()
    {
        public string Path { get; set; }
        public T Data { get; private set; }

        ~Persistent()
        {
            Dispose();
        }

        public Persistent(string path, T data = default(T))
        {
            Path = path;
            Data = data;
            if (Data is INotifyPropertyChanged npc)
                npc.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Save();
        }

        public void Save(string path = null)
        {
            if (path == null)
                path = Path;

            var ser = new XmlSerializer(typeof(T));
            using (var f = File.Create(path))
            {
                ser.Serialize(f, Data);
            }
        }

        public static Persistent<T> Load(string path, bool saveIfNew = true)
        {
            var config = new Persistent<T>(path, new T());

            if (File.Exists(path))
            {
                var ser = new XmlSerializer(typeof(T));
                using (var f = File.OpenRead(path))
                {
                    config.Data = (T)ser.Deserialize(f);
                }
            }
            else if (saveIfNew)
            {
                config.Save(path);
            }

            return config;
        }

        public void Dispose()
        {
            try
            {
                if (Data is INotifyPropertyChanged npc)
                    npc.PropertyChanged -= OnPropertyChanged;
                Save();
            }
            catch
            {
                // ignored
            }
        }
    }

}

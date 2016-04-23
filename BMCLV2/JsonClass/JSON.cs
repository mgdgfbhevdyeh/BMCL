﻿using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace BMCLV2.JsonClass
{
    public class JSON
    {
        private readonly DataContractJsonSerializer _serialzier;

        public JSON(Type T)
        {
            _serialzier = new DataContractJsonSerializer(T);
        }

        public object Parse(Stream stream)
        {
            return _serialzier.ReadObject(stream);
        }

        public object Parse(string json)
        {
            return Parse(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        }

        public string Stringify(object obj)
        {
            var stream = new MemoryStream();
            _serialzier.WriteObject(stream, obj);
            var sr = new StreamReader(stream, Encoding.UTF8);
            return sr.ReadToEnd();
        }
    }
}
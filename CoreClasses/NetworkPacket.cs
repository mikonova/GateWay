using System;
using System.Collections.Generic;
using System.Text;

namespace CoreClasses
{
    public class NetworkPacket
    {
        /// <summary>
        /// Входящий сетевой пакет.
        /// </summary>
        
        /// <summary>Сырые байты тела пакета.</summary>
        public byte[] Data { get; init; } = Array.Empty<byte>();

        /// <summary>Адрес отправителя (ip:port).</summary>
        public string SenderEndPoint { get; init; } = string.Empty;

        /// <summary>Время получения (UTC).</summary>
        public DateTime ReceivedAt { get; init; }
        
    }
}

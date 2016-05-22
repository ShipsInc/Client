using System;
using System.Collections.Generic;
using System.Reflection;

namespace ShipsClient.Protocol.Parser
{
    class Handler
    {
        private static readonly Dictionary<Opcode, Action<Packet>> Handlers = new Dictionary<Opcode, Action<Packet>>();

        public static void LoadHandlers()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsPublic)
                    continue;

                var methods = type.GetMethods();

                foreach (MethodInfo method in methods)
                {
                    if (!method.IsPublic)
                        continue;

                    var attrs = (ParserAttribute[])method.GetCustomAttributes(typeof(ParserAttribute), false);

                    if (attrs.Length <= 0)
                        continue;

                    var parms = method.GetParameters();

                    if (parms.Length <= 0)
                        continue;

                    if (parms[0].ParameterType != typeof(Packet))
                        continue;

                    foreach (ParserAttribute attr in attrs)
                    {
                        Opcode opc = attr.Opcode;
                        if (opc == Opcode.NULL_OPCODE)
                            continue;

                        var del = (Action<Packet>)Delegate.CreateDelegate(typeof(Action<Packet>), method);
                        Handlers[opc] = del;
                    }
                }
            }
        }

        public static void SelectHandler(Packet packet)
        {
            if (packet.Opcode == 0)
                return;

            Action<Packet> handler;
            var hasHandler = Handlers.TryGetValue(packet.Opcode, out handler);
            if (!hasHandler)
            {
                Console.WriteLine($"Not fond handler for opcode {packet.Opcode}");
                return;
            }

            handler(packet);
        }
    }
}

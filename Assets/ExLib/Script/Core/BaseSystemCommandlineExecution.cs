using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExLib
{
#if UNITY_STANDALONE_WIN
    public class BaseSystemCommandlineExecution
    {
        private static string[] _commands;
        private static Hashtable _hash = new Hashtable();

        public static bool ContainCommand(string cmd)
        {
            if (_commands == null)
                _commands = System.Environment.GetCommandLineArgs();

            cmd = cmd[0] != '-' ? "-" + cmd : cmd;

            foreach (string arg in _commands)
            {
                if (arg.Equals(cmd))
                    return true;
            }

            return false;
        }

        public static void AddCommand(string cmd, System.Action<string> callback)
        {
            if (!AddCommandAlready(cmd))
                _hash.Add("-" + cmd, callback);
            else
                _hash["-" + cmd] = callback;
        }

        public static void AddCommand(string cmd, System.Action callback)
        {
            if (!AddCommandAlready(cmd))
                _hash.Add("-" + cmd, callback);
            else
                _hash["-" + cmd] = callback;
        }

        public static void RemoveCommand(string cmd)
        {
            _hash.Remove("-" + cmd);
        }

        public static bool AddCommandAlready(string cmd)
        {
            return _hash.Contains("-" + cmd);
        }

        public static void Excute()
        {
            if (_commands == null)
                _commands = System.Environment.GetCommandLineArgs();

            if (_commands == null || _commands.Length <= 0)
                return;

            for (int i = 0, len = _commands.Length; i < len; i++)
            {
                if (_hash.ContainsKey(_commands[i]))
                {
                    System.Action<string> callbackParam = _hash[_commands[i]] as System.Action<string>;
                    if (callbackParam == null)
                    {
                        System.Action callback = (System.Action)_hash[_commands[i]];
                        if (callback == null)
                        {
                            continue;
                        }
                        else
                        {
                            callback.Invoke();
                        }
                    }
                    else
                    {
                        callbackParam.Invoke(_commands[i + 1]);
                    }
                }
            }
        }
    }
#endif
}

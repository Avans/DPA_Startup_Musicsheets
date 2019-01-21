﻿using System;
using System.Collections.Generic;

namespace DPA_Musicsheets.Models.Events
{
    public class OwnEventmanager
    {
        private static OwnEventmanager instance;
        private Dictionary<string, Event<object>> events;

        private OwnEventmanager()
        {
            events = new Dictionary<string, Event<object>>
            {
                {"addLilyPondToken", new Event<object>() }  ,
                {"changeInformativeText", new Event<object>() },
                {"changeEditorState", new Event<object>() },
                {"changePlayerState", new Event<object>() },
                {"onClose", new Event<object>() },
                {"setLilypond", new Event<object>() },
                {"setStaffs", new Event<object>() },
            };
        }

        public void DispatchEvent(string name, object argument)
        {
            events[name].Dispatch(argument);
        }

        public void Subscribe(string name, Action<object> callback)
        {
            events[name].Subscribe(callback);
        }

        public static OwnEventmanager Manager
        {
            get
            {
                if (instance == null)
                {
                    instance = new OwnEventmanager();
                }
                return instance;
            }
        }
    }
}

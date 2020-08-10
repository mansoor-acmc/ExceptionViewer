using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace ExceptionViewer
{
    
    public class DeviceMessage
    {
    
        public string Message { get; set; }
        public string StackTrace { get; set; }
    
        public string MethodName { get; set; }

    
        public DateTime DateOccur { get; set; }

        public string DateOccurString { get; set; }
    
        public string Username { get; set; }

    
        public string DeviceName { get; set; }

    
        public string  ProjectName { get; set; }

    
        public string DeviceIP { get; set; }

    
        public bool IsSaved { get; set; }

    
        public object Parameters { get; set; }

    
        public Int64 ID { get; set; }
    }
}
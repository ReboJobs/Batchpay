
using System;
using System.ComponentModel;
using System.Reflection;


namespace XeroApp.Enums
{
    public enum EnumReportBugStatus : byte
    {
        [Description("Open")]
        Open = 1,
        [Description("In-Progress")]
        InProgress = 2,
        [Description("Resolved")]
        Resolved = 3
    }

}

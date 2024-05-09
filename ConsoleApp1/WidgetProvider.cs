using Microsoft.Windows.Widgets.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{


    internal class WidgetProvider : IWidgetProvider, IWidgetProvider2
    {
        static ManualResetEvent emptyWidgetListEvent = new ManualResetEvent(false);

        public static ManualResetEvent GetEmptyWidgetListEvent()
        {
            return emptyWidgetListEvent;
        }

        public static Dictionary<string, WidgetInfo> RunningWidgets = new Dictionary<string, WidgetInfo>();

        const string countWidgetTemplate = """
{                                                                     
    "type": "AdaptiveCard",                                         
    "body": [                                                         
        {                                                               
            "type": "TextBlock",                                    
            "text": "You are in normal mode."    
        }                                                            
    ],                                                                  
    "actions": [                                                            
    ],                                                                  
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.5"                                                
}
""";
        const string customizationWidgetTemplate = """
{                                                                     
    "type": "AdaptiveCard",                                         
    "body": [                                                         
        {                                                               
            "type": "TextBlock",                                    
            "text": "You are in edit mode."    
        }                                                            
    ],                                                                  
    "actions": [                                                      
        {                                                               
            "type": "Action.Execute",                               
            "title": "Exit",                                   
            "verb": "exit"                                           
        }                                                               
    ],                                                                  
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.5"                                                
}
""";

        public void CreateWidget(WidgetContext widgetContext)
        {
            var widgetId = widgetContext.Id; // To save RPC calls
            var widgetName = widgetContext.DefinitionId;
            var runningWidgetInfo = new WidgetInfo() { widgetId = widgetId, customizationMode = false };
            RunningWidgets[widgetId] = runningWidgetInfo;


            // Update the widget
            UpdateWidget(runningWidgetInfo);
        }

        public void DeleteWidget(string widgetId, string customState)
        {
            RunningWidgets.Remove(widgetId);
            if (RunningWidgets.Count == 0)
            {
                emptyWidgetListEvent.Set();
            }
        }
        public void OnCustomizationRequested(WidgetCustomizationRequestedArgs customizationInvokedArgs)
        {
            var widgetId = customizationInvokedArgs.WidgetContext.Id;
            if (RunningWidgets.ContainsKey(widgetId))
            {
                var localWidgetInfo = RunningWidgets[widgetId];
                localWidgetInfo.customizationMode = true;
                UpdateWidget(localWidgetInfo);
            }
        }
        public void OnActionInvoked(WidgetActionInvokedArgs actionInvokedArgs)
        {
            var verb = actionInvokedArgs.Verb;
            if (verb == "exit")
            {
                var widgetId = actionInvokedArgs.WidgetContext.Id;
                // If you need to use some data that was passed in after
                // Action was invoked, you can get it from the args:
                var data = actionInvokedArgs.Data;
                if (RunningWidgets.ContainsKey(widgetId))
                {
                    var localWidgetInfo = RunningWidgets[widgetId];
                    // Increment the count
                    localWidgetInfo.customizationMode = false;
                    UpdateWidget(localWidgetInfo);
                }
            }
        }
        public void OnWidgetContextChanged(WidgetContextChangedArgs contextChangedArgs)
        {
            var widgetContext = contextChangedArgs.WidgetContext;
            var widgetId = widgetContext.Id;
            var widgetSize = widgetContext.Size;
            if (RunningWidgets.ContainsKey(widgetId))
            {
                var localWidgetInfo = RunningWidgets[widgetId];
                UpdateWidget(localWidgetInfo);
            }
        }
        public void Activate(WidgetContext widgetContext)
        {
            var widgetId = widgetContext.Id;

            if (RunningWidgets.ContainsKey(widgetId))
            {
                var localWidgetInfo = RunningWidgets[widgetId];

                UpdateWidget(localWidgetInfo);
            }
        }
        public void Deactivate(string widgetId)
        {
            if (RunningWidgets.ContainsKey(widgetId))
            {
                var localWidgetInfo = RunningWidgets[widgetId];
                UpdateWidget(localWidgetInfo);
            }
        }

        void UpdateWidget(WidgetInfo localWidgetInfo)
        {
            WidgetUpdateRequestOptions updateOptions = new WidgetUpdateRequestOptions(localWidgetInfo.widgetId);

            string? templateJson = null;

            if (localWidgetInfo.customizationMode)
            {
                templateJson = customizationWidgetTemplate;
            }
            else
            {
                templateJson = countWidgetTemplate;
            }

            string? dataJson = "{}";

            updateOptions.Template = templateJson;
            updateOptions.Data = dataJson;
            WidgetManager.GetDefault().UpdateWidget(updateOptions);
        }
    }

    internal class WidgetInfo
    {
        public string? widgetId { get; set; }
        public bool customizationMode { get; set; }
    }
}

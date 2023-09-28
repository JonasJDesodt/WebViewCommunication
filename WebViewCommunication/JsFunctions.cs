using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebViewCommunication
{
    public class JsFunctions
    {
        public string GetAddDivFunction()
        {
            var builder = new StringBuilder();
            builder.Append("function AddDiv(x, y){");
            builder.Append("const div = document.createElement(\'div\');");
            builder.Append("div.style.position = \'absolute\';");
            builder.Append("div.style.left = x + \'px\';");
            builder.Append("div.style.top = y + \'px\';");
            builder.Append("div.style.width = \'250px\';");
            builder.Append("div.style.height = \'250px\';");
            builder.Append("div.style.background = \'red\';");
            builder.Append("document.getElementsByTagName(\'body\')[0].insertBefore(div, document.getElementsByTagName(\'body\')[0].getElementsByTagName(\'script\')[0]);");
            builder.Append("};");

            return builder.ToString();
        }

        public string GetAddFormFunction()
        {
            var builder = new StringBuilder();
            builder.Append("function AddForm(){");
            builder.Append("const form = document.createElement(\'form\');");
            builder.Append("form.addEventListener(\'submit\', function(e) {  e.preventDefault(); const data = new FormData(form); window.chrome.webview.postMessage(Object.fromEntries(data)); });");
            //possible issues w/ object.FromEntries() => e.g. multiple values w/ the same name, checkboxes & raduibutton, multiselect select

            var id1 = Guid.NewGuid();

            builder.Append("let label = document.createElement(\'label\');");
            builder.Append("label.innerHTML = \'input-1\';");
            builder.Append($"label.for = \'{id1}\';");
            builder.Append("form.appendChild(label);");

            builder.Append("let input = document.createElement(\'input\');");
            builder.Append("input.type = \'text\';");
            builder.Append($"input.id = \'{id1}\';");
            builder.Append("input.name = \'input-1\';");
            builder.Append("form.appendChild(input);");

            var id2 = Guid.NewGuid();

            builder.Append("label = document.createElement(\'label\');");
            builder.Append("label.innerHTML = \'input-2\';");
            builder.Append($"label.for = \'{id2}\';");
            builder.Append("form.appendChild(label);");

            builder.Append("input = document.createElement(\'input\');");
            builder.Append("input.type = \'text\';");
            builder.Append($"input.id = \'{id2}\';");
            builder.Append("input.name = \'input-2\';");
            builder.Append("form.appendChild(input);");

            builder.Append("const button = document.createElement(\'input\');");
            builder.Append("button.type=\'submit\';");
            builder.Append("button.value=\'Send\';");
            builder.Append("form.appendChild(button);");

            builder.Append("document.getElementsByTagName(\'body\')[0].insertBefore(form, document.getElementsByTagName(\'body\')[0].getElementsByTagName(\'script\')[0]);");
            builder.Append("}");

            return builder.ToString();
        }

        public string GetReceiveWebMessageFunction()
        {
            var builder = new StringBuilder();
            builder.Append("window.chrome.webview.addEventListener(\'message\', event => {");
            builder.Append("alert(event.data.Url);");
            builder.Append("});");
       
            return builder.ToString();
        }

        public string GetScriptAppender()
        {
            var scriptBuilder = new StringBuilder();

            scriptBuilder.Append("const script = document.createElement('script');");
            scriptBuilder.Append("script.type = 'text/javascript';");

            var contentBuilder = new StringBuilder();
            //contentBuilder.Append("const bridge = chrome.webview.hostObjects.bridge;");
            contentBuilder.Append("async function addScript(func){"); // adds the function to add scripts
            contentBuilder.Append("console.log(func);");
            contentBuilder.Append("const script = document.createElement(\"script\");");
            //contentBuilder.Append("script.innerHTML = await bridge[func];");
            contentBuilder.Append("script.innerHTML = func;");

            contentBuilder.Append("document.getElementsByTagName(\"body\")[0].appendChild(script);");
            contentBuilder.Append("};");

            scriptBuilder.Append($"script.innerHTML = '{contentBuilder}';");
            scriptBuilder.Append($"document.getElementsByTagName('head')[0].appendChild(script);");

            return scriptBuilder.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Customization;

namespace IgorKL.ACAD3.Model.Extensions {
    public static class CustomizationExtensions {

        public static RibbonTabSource AddNewTab(
        this CustomizationSection instance,
        string name,
        string text = null) {

            if (text == null)
                text = name;

            var ribbonRoot = instance.MenuGroup.RibbonRoot;
            var id = "tab" + name;
            var ribbonTabSource = new RibbonTabSource(ribbonRoot);

            ribbonTabSource.Name = name;
            ribbonTabSource.Text = text;
            ribbonTabSource.Id = id;
            ribbonTabSource.ElementID = id;

            ribbonRoot.RibbonTabSources.Add(ribbonTabSource);

            return ribbonTabSource;
        }

        public static RibbonPanelSource AddNewPanel(
        this RibbonTabSource instance,
        string name,
        string text = null) {

            if (text == null)
                text = name;

            var ribbonRoot = instance.CustomizationSection.MenuGroup.RibbonRoot;
            var panels = ribbonRoot.RibbonPanelSources;
            var id = "pnl" + name;
            var ribbonPanelSource = new RibbonPanelSource(ribbonRoot);

            ribbonPanelSource.Name = name;
            ribbonPanelSource.Text = text;
            ribbonPanelSource.Id = id;
            ribbonPanelSource.ElementID = id;

            panels.Add(ribbonPanelSource);

            var ribbonPanelSourceReference = new RibbonPanelSourceReference(instance);

            ribbonPanelSourceReference.PanelId = ribbonPanelSource.ElementID;

            instance.Items.Add(ribbonPanelSourceReference);

            return ribbonPanelSource;
        }

        public static RibbonRow AddNewRibbonRow(this RibbonPanelSource instance) {
            var row = new RibbonRow(instance);

            instance.Items.Add(row);

            return row;
        }

        public static RibbonRow AddNewRibbonRow(this RibbonRowPanel instance) {
            var row = new RibbonRow(instance);

            instance.Items.Add(row);

            return row;
        }

        public static RibbonRowPanel AddNewPanel(this RibbonRow instance) {
            var row = new RibbonRowPanel(instance);

            instance.Items.Add(row);

            return row;
        }

        public static RibbonCommandButton AddNewButton(
        this RibbonRow instance,
        string text,
        string commandFriendlyName,
        string command,
        string commandDescription,
        string smallImagePath,
        string largeImagePath,
        RibbonButtonStyle style) {

            var customizationSection = instance.CustomizationSection;
            var macroGroups = customizationSection.MenuGroup.MacroGroups;

            MacroGroup macroGroup;

            if (macroGroups.Count == 0)
                macroGroup = new MacroGroup("MacroGroup", customizationSection.MenuGroup);
            else
                macroGroup = macroGroups[0];

            var button = new RibbonCommandButton(instance);
            button.Text = text;

            var commandMacro = "^C^C_" + command;
            var commandId = "ID_" + command;
            var buttonId = "btn" + command;
            var labelId = "lbl" + command;

            var menuMacro = macroGroup.CreateMenuMacro(commandFriendlyName,
            commandMacro,
            commandId,
            commandDescription,
            MacroType.Any,
            smallImagePath,
            largeImagePath,
            labelId);
            var macro = menuMacro.macro;
            /*Associate custom command to ribbonbutton macro*/
            macro.CLICommand = command;

            button.MacroID = menuMacro.ElementID;
            button.ButtonStyle = style;
            instance.Items.Add(button);

            return button;
        }

    }
}

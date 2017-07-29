using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using IgorKL.ACAD3.Model.Extensions;

namespace IgorKL.ACAD3.Model
{
    public static class AcadEnvironments
    {
        public static System.Globalization.CultureInfo Culture { get { return System.Globalization.CultureInfo.GetCultureInfo("en-US"); } }

        public static Editor Editor { get; } =
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

        public static Document ActiveAcadDocument { get; } =
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

        public static Database Database { get; } = HostApplicationServices.WorkingDatabase;
    }

}

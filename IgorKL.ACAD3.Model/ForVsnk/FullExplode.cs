using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;

using IgorKL.ACAD3.Model.Extensions;

namespace IgorKL.ACAD3.Model.ForVsnk
{
    public class FullExplode
    {
        [RibbonCommandButton("РасчленитьВсе", "Стрелки")]
        [Autodesk.AutoCAD.Runtime.CommandMethod("iCmd_FullExplode", Autodesk.AutoCAD.Runtime.CommandFlags.UsePickSet)]
        public static void FullExplodeCmd()
        {
            List<Entity> entities = new List<Entity>();
            List<ObjectId> explodedList = new List<ObjectId>();
            var editor = Tools.GetAcadEditor();
            Database db = HostApplicationServices.WorkingDatabase;
            /*Tools.StartTransaction(trans =>
            {
                RemoveProxyEntities.RemoveProxiesFromDictionary(db.NamedObjectsDictionaryId, trans);
            });*/
            var sres = editor.SelectAll();
            if (sres.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
            {
                var ids = sres.Value.GetObjectIds().ToList();
                Tools.StartTransaction(() =>
                {
                    explodedList = ids.Where(id =>
                    {
                        if (id == null || id.IsNull || id.IsErased)
                            return false;
                        Entity ent = id.GetObject(OpenMode.ForWrite, false, false) as Entity;
                        if (ent != null)
                        {
                            DBObjectCollection acDBObjColl = new DBObjectCollection();
                            List<Entity> buffer = wheleExplode(ent);
                            if (buffer.Count > 0)
                            {
                                Tools.AppendEntity2(buffer);
                                return true;
                            }
                        }
                        return false;
                    }).ToList();
                    
                });
                Tools.StartTransaction(() =>
                {
                    explodedList.ForEach(id =>
                    {
                        Entity ent = id.GetObject(OpenMode.ForWrite, false, false) as Entity;
                        ent.Erase(true);
                    });
                });
            }
        }

        private static List<Entity> wheleExplode(Entity ent)
        {
            List<Entity> res = new List<Entity>();
            DBObjectCollection acDBObjColl = new DBObjectCollection();
            try
            {
                ent.Explode(acDBObjColl);
            }
            catch (Autodesk.AutoCAD.Runtime.Exception acadError) {
                System.Diagnostics.Debug.Write($"\n{acadError}\n{acadError.ErrorStatus}\n{ent.ToString()}", "Explode error");
                if (ent.IsNewObject)
                    res.Add(ent);
                return res;
            }
            List<Entity> buffer = new List<Entity>(acDBObjColl.Cast<Entity>());
            foreach (Entity item in buffer)
            {
                res.AddRange(wheleExplode(item));
            }
            return res;
        }
    }
}

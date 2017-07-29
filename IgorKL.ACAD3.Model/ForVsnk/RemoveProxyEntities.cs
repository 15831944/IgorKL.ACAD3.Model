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

using Autodesk.AutoCAD.Runtime;
using acApp = Autodesk.AutoCAD.ApplicationServices.Application;

using IgorKL.ACAD3.Model.Extensions;

namespace IgorKL.ACAD3.Model.ForVsnk
{

    public class RemoveProxyEntities
    {
        public static void RemoveEntry(
          DBDictionary dict, ObjectId id, Transaction tr)
        {
            ProxyObject obj =
              (ProxyObject)tr.GetObject(id, OpenMode.ForRead);

            // If you want to check what exact proxy it is
            /*if (obj.OriginalClassName != "ProxyToRemove")
                return;*/

            dict.Remove(id);
        }

        public static void RemoveEntry2(
          DBDictionary dict, ObjectId id, Transaction tr)
        {
            ProxyObject obj =
              (ProxyObject)tr.GetObject(id, OpenMode.ForRead);

            // If you want to check what exact proxy it is
            /*if (obj.OriginalClassName != "ProxyToRemove")
                return;*/

            obj.UpgradeOpen();

            using (DBObject newObj = new Xrecord())
            {
                obj.HandOverTo(newObj, false, false);
                newObj.Erase();
            }
        }

        public static void RemoveProxiesFromDictionary(
          ObjectId dictId, Transaction tr)
        {
            using (ObjectIdCollection ids = new ObjectIdCollection())
            {
                DBDictionary dict =
                  (DBDictionary)tr.GetObject(dictId, OpenMode.ForRead);

                foreach (DBDictionaryEntry entry in dict)
                {
                    RXClass c1 = entry.Value.ObjectClass;
                    RXClass c2 = RXClass.GetClass(typeof(ProxyObject));

                    if (entry.Value.ObjectClass.Name == "AcDbZombieObject")
                        ids.Add(entry.Value);
                    else if (entry.Value.ObjectClass ==
                      RXClass.GetClass(typeof(DBDictionary)))
                        RemoveProxiesFromDictionary(entry.Value, tr);
                }

                if (ids.Count > 0)
                {
                    dict.UpgradeOpen();

                    foreach (ObjectId id in ids)
                        RemoveEntry2(dict, id, tr);
                }
            }
        }

        [CommandMethod("RemoveProxiesFromNOD", "RemoveProxiesFromNOD",
          CommandFlags.Modal)]
        public static void RemoveProxiesFromNOD()
        {
            Database db = HostApplicationServices.WorkingDatabase;

            // Help file says the following about HandOverTo:
            // "This method is not allowed on objects that are
            // transaction resident.
            // If the object on which the method is called is transaction
            // resident, then no handOverTo operation is performed."
            // That's why we need to use Open/Close transaction
            // instead of the normal one
            using (Transaction tr =
              db.TransactionManager.StartOpenCloseTransaction())
            {
                RemoveProxiesFromDictionary(db.NamedObjectsDictionaryId, tr);

                tr.Commit();
            }
        }

        [CommandMethod("RemoveProxiesFromBlocks", "RemoveProxiesFromBlocks",
          CommandFlags.Modal)]
        public static void RemoveProxiesFromBlocks()
        {
            Database db = HostApplicationServices.WorkingDatabase;

            using (Transaction tr =
              db.TransactionManager.StartOpenCloseTransaction())
            {
                BlockTable bt =
                  (BlockTable)tr.GetObject(
                    db.BlockTableId, OpenMode.ForRead);

                foreach (ObjectId btrId in bt)
                {
                    BlockTableRecord btr =
                      (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);

                    foreach (ObjectId entId in btr)
                    {
                        if (entId.ObjectClass.Name == "AcDbZombieEntity")
                        {
                            ProxyEntity ent =
                              (ProxyEntity)tr.GetObject(entId, OpenMode.ForRead);

                            // If you want to check what exact proxy it is
                            if (ent.ApplicationDescription != "ProxyToRemove")
                                return;

                            ent.UpgradeOpen();

                            using (DBObject newEnt = new Line())
                            {
                                ent.HandOverTo(newEnt, false, false);
                                newEnt.Erase();
                            }
                        }
                    }
                }

                tr.Commit();
            }
        }
    }
}

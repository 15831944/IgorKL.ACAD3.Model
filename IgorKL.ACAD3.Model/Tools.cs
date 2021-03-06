﻿using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using IgorKL.ACAD3.Model.Extensions;
using System.Threading.Tasks;

namespace IgorKL.ACAD3.Model {
    public delegate ObjectId TransactionProcess<T>(T obj, Transaction trans, bool commit);
    public delegate void TransactionOpenCloseProcess<T>(T obj, Transaction trans, bool commit);

    public static class Tools {
        public static System.Globalization.CultureInfo Culture { get { return System.Globalization.CultureInfo.GetCultureInfo("en-US"); } }

        public static Editor GetAcadEditor() {
            return Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
        }

        public static Document GetActiveAcadDocument() {
            return Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
        }

        public static Autodesk.Civil.ApplicationServices.CivilDocument GetActiveCivilDocument() {
            return Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
        }

        public static Transaction StartTransaction() {
            var db = HostApplicationServices.WorkingDatabase;
            return StartTransaction(db);
        }

        public static Transaction StartOpenCloseTransaction() {
            var db = HostApplicationServices.WorkingDatabase;
            return StartOpenCloseTransaction(db);
        }

        public static BlockTableRecord GetAcadBlockTableRecordCurrentSpace(OpenMode mode = OpenMode.ForWrite) {
            return Tools.GetAcadDatabase().CurrentSpaceId.GetObject<BlockTableRecord>(mode);
        }

        public static BlockTableRecord GetAcadBlockTableRecordCurrentSpace(Transaction trans, OpenMode mode = OpenMode.ForWrite) {
            return GetAcadBlockTableRecordCurrentSpace(trans, GetActiveAcadDocument().Database, mode);
        }

        public static BlockTableRecord GetAcadBlockTableRecordCurrentSpace(Transaction trans, Database db, OpenMode mode = OpenMode.ForWrite) {
            return (BlockTableRecord)trans.GetObject(db.CurrentSpaceId, mode);
        }

        public static BlockTableRecord GetAcadBlockTableRecordModelSpace(Transaction trans, Database db, OpenMode mode = OpenMode.ForWrite) {
            BlockTable acBlkTbl;
            acBlkTbl = trans.GetObject(db.BlockTableId,
                                            OpenMode.ForRead) as BlockTable;
            BlockTableRecord acBlkTblRec;
            acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                            mode) as BlockTableRecord;

            return acBlkTblRec;
        }

        public static BlockTableRecord GetAcadBlockTableRecordModelSpace(OpenMode mode = OpenMode.ForWrite) {
            Database db = Tools.GetAcadDatabase();

            BlockTable acBlkTbl;
            acBlkTbl = db.BlockTableId.GetObjectForRead<BlockTable>();

            BlockTableRecord acBlkTblRec;
            acBlkTblRec = acBlkTbl[BlockTableRecord.ModelSpace].GetObject<BlockTableRecord>(mode);

            return acBlkTblRec;
        }

        public static BlockTableRecord GetAcadBlockTableRecordModelSpace(Transaction trans, OpenMode mode = OpenMode.ForWrite) {
            return GetAcadBlockTableRecordModelSpace(trans, GetActiveAcadDocument().Database, mode);
        }

        public static void AppendEntityEx(Transaction trans, IEnumerable<Entity> entities) {
            BlockTableRecord btr = GetAcadBlockTableRecordCurrentSpace(trans, OpenMode.ForWrite);
            foreach (var e in entities) {
                btr.AppendEntity(e);
                trans.AddNewlyCreatedDBObject(e, true);
            }
            trans.Commit();
        }

        public static ObjectId AppendEntityEx(Transaction trans, Entity entity, bool comit = true) {
            Database db = GetActiveAcadDocument().Database;
            return AppendEntityEx(trans, db, entity, comit);
        }

        public static ObjectId AppendEntityEx(Transaction trans, Database db, Entity entity, bool comit = true) {
            BlockTableRecord btr = GetAcadBlockTableRecordCurrentSpace(trans, db, OpenMode.ForWrite);
            ObjectId id = btr.AppendEntity(entity);
            trans.AddNewlyCreatedDBObject(entity, true);
            if (comit)
                trans.Commit();
            return id;
        }

        public static List<ObjectId> AppendEntity<T>(IEnumerable<T> entities)
            where T : Entity {
            List<ObjectId> res = new List<ObjectId>();
            Database db = AcadEnvironments.Database;
            bool isToplevelTrans = db.TransactionManager.NumberOfActiveTransactions > 0;
            Tools.StartTransaction((trans, doc, blockTbl, blockTblRec) => {
                foreach (var ent in entities) {
                    res.Add(blockTblRec.AppendEntity(ent));
                    trans.AddNewlyCreatedDBObject(ent, true);
                }
                return true;
            }, (err, trans) => {
                return !isToplevelTrans;
            });
            return res;
        }


        /*public static ObjectId AppendEntity(Database db, Entity entity)
        {
            using (Transaction trans = StartTransaction(db))
            {
                return AppendEntity(trans, entity, true);
            }
        }*/

        public static ObjectId AppendEntityEx(Document doc, Entity entity) {
            Database db = doc.Database;
            return AppendEntityEx(db, entity);
        }

        public static ObjectId AppendEntityEx(Database db, Entity entity) {
            using (Transaction trans = db.TransactionManager.StartTransaction()) {
                BlockTable acBlkTbl;
                acBlkTbl = trans.GetObject(db.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                ObjectId id = acBlkTblRec.AppendEntity(entity);
                trans.AddNewlyCreatedDBObject(entity, true);

                trans.Commit();

                return id;
            }
        }

        public static ObjectId AppendEntityEx(Entity entity) {
            var db = Tools.GetAcadDatabase();
            using (Transaction trans = db.TransactionManager.StartTransaction()) {
                BlockTable acBlkTbl;
                acBlkTbl = trans.GetObject(db.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                ObjectId id = acBlkTblRec.AppendEntity(entity);
                trans.AddNewlyCreatedDBObject(entity, true);

                trans.Commit();

                return id;
            }
        }


        public static Database GetAcadWorkingDatabase() {
            return HostApplicationServices.WorkingDatabase;
        }


        public static Database GetAcadDatabase() {
            //return Application.DocumentManager.MdiActiveDocument.Database;
            return HostApplicationServices.WorkingDatabase;
        }

        public static Transaction StartTransaction(Database db) {
            return db.TransactionManager.StartTransaction();
        }

        public static Transaction StartOpenCloseTransaction(Database db) {
            return db.TransactionManager.StartOpenCloseTransaction();
        }

        public static void Write(string msg) {
            var _editor = GetAcadEditor();
            _editor.WriteMessage(msg);
        }

        public static void CloneObjects(ObjectIdCollection sourceIds, Database sourceDb, Database destDb) {
            ObjectId sourceMsId = SymbolUtilityServices.GetBlockModelSpaceId(sourceDb);
            ObjectId destDbMsId = SymbolUtilityServices.GetBlockModelSpaceId(destDb);

            IdMapping mapping = new IdMapping();

            sourceDb.WblockCloneObjects(sourceIds, destDbMsId, mapping, DuplicateRecordCloning.Ignore, false);
        }

        public static string GetLocalRootPath() {
            return Application.GetSystemVariable("LOCALROOTPREFIX") as string;
        }

        public static Document CreateNewDocument() {
            // Change the file and path to match a drawing template on your workstation
            string localRoot = Application.GetSystemVariable("LOCALROOTPREFIX") as string;
            string templatePath = localRoot + "Template\\acad.dwt";

            DocumentCollection acDocMgr = Application.DocumentManager;
            Document acNewDoc = acDocMgr.Add(templatePath);
            return acNewDoc;
        }

        [Obsolete]
        public static ObjectId StartTransaction<T>(T state, TransactionProcess<T> transProcess) {
            using (Transaction trans = StartTransaction()) {
                return transProcess(state, trans, true);
            }
        }
        [Obsolete]
        public static void StartOpenCloseTransaction<T>(T state, TransactionOpenCloseProcess<T> transProcess) {
            using (Transaction trans = StartOpenCloseTransaction()) {
                transProcess(state, trans, true);
            }
        }
        [Obsolete]
        public static void StartTransactionEx(Action process) {
            using (Transaction trans = StartTransaction()) {
                process();
                trans.Commit();
            }
        }
        public static void StartTransaction(Action process) {
            StartTransaction((trans, doc) => {
                process();
            });
        }
        public static void StartTransaction(Action<Transaction, Document> process) {
            StartTransaction((trans, doc, acBlkTbl, acBlkTblRec) => {
                process(trans, doc);
                return true;    // Commit == true, виксирует изменения в транзакции
            }, (err, trans) => {
                return true;    // Dispose with abort == true
            });
        }
        [Obsolete]
        public static void StartOpenCloseTransactionEx(Action process) {
            using (Transaction trans = StartOpenCloseTransaction()) {
                process();
            }
        }
        public static void StartOpenCloseTransaction(Action process) {
            var db = AcadEnvironments.Database;
            Transaction trans = db.TransactionManager.StartOpenCloseTransaction();
            try {
                process();
                trans.Commit();
            } catch (Exception ex) {
                System.Diagnostics.Debug.Write($"\n{ex.Message}\n{ex.StackTrace}\n{process.ToString()}", "Transaction error");
                System.Diagnostics.Debug.Print($"Transaction error - {process.ToString()}");
            } finally {
                if (trans != null && !trans.IsDisposed) {
                    try {
                        trans.Abort();
                    } catch (Exception) { }
                    trans.Dispose();
                }
                trans = null;
            }
        }

        /// <summary>
        /// Выполняет транзакцию
        /// </summary>
        /// <param name="process">Процесс выполнения (return - commit == true)</param>
        /// <param name="callback">Процедура обратного вызова, анализ ошибок (return dispose with abort == true)</param>
        public static void StartTransaction(Func<Transaction, Document, BlockTable, BlockTableRecord, bool> process, Func<Exception, Transaction, bool> callback = null, bool isOpenCloseTrans = false) {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            bool commit = true;
            Exception error = null;

            Transaction trans = !isOpenCloseTrans ? db.TransactionManager.StartTransaction()
                : db.TransactionManager.StartOpenCloseTransaction();
#if DEBUG
            Tools.Write($"\nLog - {trans.AutoDelete}\n");
#endif
            try {
                BlockTable acBlkTbl = trans.GetObject(db.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;
                commit = process != null && process(trans, doc, acBlkTbl, acBlkTblRec);
                if (commit) {
                    try {
                        trans.Commit();
                    } catch (Exception ex) {
                        Tools.Write($"\nCommit error (from transaction) - {ex.Message}\n");
                        try {
                            trans.Abort();
                        } catch (Exception abortEx) {
                            Tools.Write($"\nAbort exception (from transaction) - {abortEx.Message}\n");
                        }
                    }
                }

            } catch (Autodesk.AutoCAD.Runtime.Exception acadError) {
                Tools.Write($"\n{acadError.Message}\n{acadError.ErrorStatus}");
            } catch (Exception ex) {
                System.Diagnostics.Debug.Write($"\n{ex.Message}\n{ex.StackTrace}\n{process.ToString()}", "Transaction error");
                System.Diagnostics.Debug.Print($"Transaction error - {process.ToString()}");
                Tools.Write($"\n{ex.Message}\n");
            } finally {
                bool dispose = callback == null ? true :
                    callback(error, trans);

                if (dispose && trans != null && !trans.IsDisposed) {
                    trans.Dispose();
                }
                trans = null;
            }
        }

        public static ViewportTableRecord GetViewportTblRec(Transaction trans, Document doc, OpenMode mode = OpenMode.ForRead) {
            ViewportTableRecord acVportTblRec = trans.GetObject(doc.Editor.ActiveViewportId,
                                  mode) as ViewportTableRecord;
            return acVportTblRec;
        }

        public static LayerTable GetLayerTable(Transaction trans, Document doc, OpenMode mode = OpenMode.ForRead) {
            LayerTable acLyrTbl;
            acLyrTbl = trans.GetObject(doc.Database.LayerTableId,
                                            mode) as LayerTable;
            return acLyrTbl;
        }

        public static Transaction GetTopTransaction() {
            return Tools.GetAcadDatabase().TransactionManager.TopTransaction;
        }


        /*public void ToggleHWAcceleration()
        {
            using (Autodesk.AutoCAD.GraphicsSystem.Configuration config =
              new Autodesk.AutoCAD.GraphicsSystem.Configuration())
            {
                bool b = config.IsFeatureEnabled(
                  Autodesk.AutoCAD.GraphicsSystem.HardwareFeature.HardwareAcceleration);
                config.SetFeatureEnabled(
                  Autodesk.AutoCAD.GraphicsSystem.
                  HardwareFeature.HardwareAcceleration, !b);
                config.SaveSettings();
            }
        }

        public void ToggleHWAcceleration()
        {
            using (Autodesk.AutoCAD.GraphicsSystem.Configuration config =
              new Autodesk.AutoCAD.GraphicsSystem.Configuration())
            {
                config.setHardwareAcceleration(true);
            }
        }*/

#if DEBUG
        [Autodesk.AutoCAD.Runtime.CommandMethod("iCMD_CreateTestCuix1")]
        public static void CreateTestCuix1() {
            var editor = AcadEnvironments.Editor;
            string sMainCuiFile = (string)Application.GetSystemVariable("MENUNAME");
            sMainCuiFile += ".cuix";
            editor.WriteMessage(sMainCuiFile);
            try {
                var debugFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var customizationSection = new Autodesk.AutoCAD.Customization.CustomizationSection();
                var menuGroup = customizationSection.MenuGroup;
                var tab = customizationSection.AddNewTab("!CuiTestTab");
                var panel = tab.AddNewPanel("Panel");
                var row = panel.AddNewRibbonRow();

                menuGroup.Name = "CuiTest1";

                row.AddNewButton(
                    "Smile",
                    "Smile",
                    "KeepSmiling",
                    "How to add BMP icon to Custom Command",
                    debugFolder + "\\smile_16.bmp",
                    debugFolder + "\\smile_32.bmp",
                    Autodesk.AutoCAD.Customization.RibbonButtonStyle.LargeWithText);


                var fileName = System.IO.Path.Combine(debugFolder, "CuiTest1.cuix");

                System.IO.File.Delete(fileName);
                customizationSection.SaveAs(fileName);

                //customizationSection.Save(true);

            } catch (Exception ex) {
                editor.WriteMessage(Environment.NewLine + ex.Message);
            }


        }
        [Autodesk.AutoCAD.Runtime.CommandMethod("KeepSmiling")]
        public static void KeepSmiling() {
            Application.ShowAlertDialog("KeepSmiling :D");
        }
#endif
    }
}

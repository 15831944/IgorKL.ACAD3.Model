using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using IgorKL.ACAD3.Model.Extensions;

namespace IgorKL.ACAD3.Model
{
    public static class Tools2
    {
        public static void StartTransaction(Action process)
        {
            var db = AcadEnvironments.Database;
            bool isToplevelTrans = db.TransactionManager.NumberOfActiveTransactions > 0;
            Transaction trans = isToplevelTrans ? db.TransactionManager.TopTransaction :  db.TransactionManager.StartTransaction();
            try
            {
                process();
                if (!isToplevelTrans)
                    trans.Commit();
            }
            catch (Autodesk.AutoCAD.Runtime.Exception acadError)
            {
                Tools.Write($"\n{acadError.Message}\n{acadError.ErrorStatus}");
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.Write($"\n{ex.Message}\n{ex.StackTrace}\n{process.ToString()}", "Transaction error");
                System.Diagnostics.Debug.Print($"Transaction error - {process.ToString()}");
                Tools.Write($"\n{ex.Message}\n");
            }
            finally
            {
                if (!isToplevelTrans)
                {
                    if (trans != null && !trans.IsDisposed)
                    {
                        try
                        {
                            trans.Abort();
                        }
                        catch (Exception) { }
                        trans.Dispose();
                    }
                    trans = null;
                }

            }
        }


    }
}

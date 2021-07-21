using GameFramework.Fsm;
using GameFramework.Procedure;

namespace UGFExtensions
{
    public class ProcedureAwaitTest: ProcedureBase
    {
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
           // var dataTable = await  GameEntry.DataTable.get
        }
    }
}
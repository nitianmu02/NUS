using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.FA.ECS.WinSECSMarshalObj;
using UMC.FA.ECS.EAP;
using UMC.FA.ECS.ED;
using UMC.FA.Kit.ED.Utility;
using CommonConstants = UMC.FA.Kit.ED.Utility.modConstants;
using System.Runtime.CompilerServices;

namespace UMC.FA.Specific.ED.Kla8900
{
    public abstract class AbstractDriverAction : IDriverAncestor
    {
        private Dictionary<string, Action<SECSTransaction>> handlers = new Dictionary<string, Action<SECSTransaction>>();
        public DriverParent Parent { set; get; }
        public VfeiMessage PrimaryVfeiMsg { private set; get; }
        public VfeiMessage SecondaryVfeiMsg { protected set; get; } = new VfeiMessage();

        internal protected virtual bool AutoReplyVfeiOnError { get; } = true;

        internal protected virtual void HandlePrimaryIn(SECSTransaction trans) { }
        internal protected virtual void HandlePrimaryOut(SECSTransaction trans) { }
        internal protected virtual void HandleSecondaryIn(SECSTransaction trans) { }
        internal protected virtual void HandleSecondaryOut(SECSTransaction trans) { }
        internal protected virtual void HandleMonitor(DateTime time, bool sent, string evt, string hexBytes) { }
        internal protected virtual void HandleSECSError(long errorCode, string errorText) { }
        internal protected virtual void HandleSECSWarning(long errorCode, string errorText) { }
        internal protected virtual void HandleDisconnect(long errorCode, string errorText) { }
        internal protected virtual void HandleSxF0Reply(SECSTransaction trans) { }
        internal protected virtual void HandleConnect() { }
        internal abstract void DoExecute();
        public void Execute(VfeiMessage vfei)
        {
            PrimaryVfeiMsg = vfei;
            SecondaryVfeiMsg = PrimaryVfeiMsg.Clone();
            try
            {
                DoExecute();
            }
            catch (Exception ex)
            {
                this.WriteTrace($"Exception caught when executing: {ex}", 0);
                if (this.AutoReplyVfeiOnError) SecondaryVfeiMsg?.Reply(Parent, 999, $"Exception caught when execute: {ex.Message}");
            }
            finally
            {
#if DEBUG
                this.WriteTrace($"Done handling {vfei.Build()}");
#endif
            }
        }
        protected void SendSECS(SECSTransaction trans, Action<SECSTransaction> replyHandler, string handlerId = "", [CallerMemberName] string memberName = "")
        {
            if (trans == null) return;
            if (trans.ReplyExpected)
            {
                var key = $"{memberName}-S{trans?.Primary.Stream}F{trans?.Secondary.Function}({trans.Description}.{trans.Primary.Root.Name})-VFEI_TID:{PrimaryVfeiMsg.TransactionID}_Handler";
                trans.ActionOnSuccess = string.IsNullOrEmpty(handlerId)? key : handlerId;
                if (handlers.ContainsKey((string)trans.ActionOnSuccess))
                {
                    handlers[(string)trans.ActionOnSuccess] = replyHandler;
                }
                else
                {
                    handlers.Add((string)trans.ActionOnSuccess, replyHandler);
                }
            }
            Parent?.SendTransaction(trans);
        }

        public void Connect()
        {
            this.WriteTrace(modTraceMsg.FormatStdMsg(modTraceMsg.enmMsgCode.SECS_CONNECT));
            try
            {
                HandleConnect();
            }
            catch (Exception ex)
            {
                this.WriteTrace($"Exception caught when handling Connect event: {ex}", 0);
            }
        }

        public void Disconnect(long errorCode, string errorText)
        {
            var logMsg = modTraceMsg.FormatStdMsg(modTraceMsg.enmMsgCode.SECS_DISCONNECT, errorCode.ToString(), errorText);
            this.WriteTrace(logMsg, 0);
            if (AutoReplyVfeiOnError) SecondaryVfeiMsg?.Reply(Parent, 1, $"Equipment driver disconnected with equipment with errorCode {errorCode} and errorText {errorText}");
            // offline
            try
            {
                HandleDisconnect(errorCode, errorText);
            }
            catch (Exception ex)
            {
                this.WriteTrace($"Exception caught when handling Disconnect event: {ex}", 0);
            }
        }
        public virtual void Monitor(DateTime time, bool sent, string evt, string hexBytes)
        {
            try
            {
                HandleMonitor(time, sent, evt, hexBytes);
            }
            catch (Exception ex)
            {
                this.WriteTrace($"Exception caught while handling secs monitor event: {ex}", 0);
                modError.HandleErrInfo(ex, Parent, true);
            }
        }

        public void PrimaryIn(SECSTransaction trans, long errorCode, string errorText)
        {
            try
            {
                if (errorCode != 0)
                {
                    if (AutoReplyVfeiOnError) SecondaryVfeiMsg?.Reply(Parent, errorCode, $"S{trans.Primary.Stream}F{trans.Primary.Function} receive error: {errorText}");
                }
                else
                {
                    HandlePrimaryIn(trans);
                }
            }
            catch(Exception ex)
            {
                this.WriteTrace($"Exception caught while handling S{trans.Primary.Stream}F{trans.Primary.Function} in HandlePrimaryIn: {ex}", 0);
                if (AutoReplyVfeiOnError) SecondaryVfeiMsg?.Reply(Parent, 999, $"Exception caught when handling PrimaryIn S{trans.Primary.Stream}F{trans.Primary.Function}: {ex.Message}");
                //modError.HandleErrInfo(ex, Parent, true);
            }
            finally
            {
#if DEBUG
                this.WriteTrace($"Done handling S{trans.Primary.Stream}F{trans.Primary.Function} in HandlePrimaryIn.");
#endif
            }
        }

        public void PrimaryOut(SECSTransaction trans, long errorCode, string errorText)
        {
            try
            {
                if (errorCode != 0)
                {
                    if (AutoReplyVfeiOnError) SecondaryVfeiMsg?.Reply(Parent, errorCode, $"S{trans.Primary.Stream}F{trans.Primary.Function} send error: {errorText}");
                }
                else
                {
                    HandlePrimaryOut(trans);
                }
            }
            catch (Exception ex)
            {
                this.WriteTrace($"Exception caught while handling S{trans.Primary.Stream}F{trans.Primary.Function} in HandlePrimaryOut: {ex}", 0);
                if (AutoReplyVfeiOnError) SecondaryVfeiMsg?.Reply(Parent, 999, $"Exception caught when handling PrimaryOut S{trans.Primary.Stream}F{trans.Primary.Function}: {ex.Message}");
                //modError.HandleErrInfo(ex, Parent, true);
            }
            finally
            {
#if DEBUG
                this.WriteTrace($"Done handling S{trans.Primary.Stream}F{trans.Primary.Function} in HandlePrimaryOut.");
#endif
            }
        }


        public void SecondaryIn(SECSTransaction trans, long errorCode, string errorText)
        {
            try
            {
                if (errorCode != 0)
                {
                    if (AutoReplyVfeiOnError) SecondaryVfeiMsg?.Reply(Parent, errorCode, $"S{trans.Primary.Stream}F{trans.Primary.Function} {trans.Primary.Root.Name} reply error: {errorText}");
                }
                else if (trans.Secondary.Function == 0)
                {
                    // offline
                    if (AutoReplyVfeiOnError) SecondaryVfeiMsg?.Reply(Parent, 1, "Equipment is offline.");
                    HandleSxF0Reply(trans);
                }
                else
                {
                    var handlerID = Convert.ToString(trans.ActionOnSuccess);
                    if (handlers.ContainsKey(handlerID))
                    {
                        handlers[handlerID](trans);
                    }
                    else
                    {
                        HandleSecondaryIn(trans);
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteTrace($"Exception caught while handling S{trans.Secondary.Stream}F{trans.Secondary.Function} in HandleSecondaryIn: {ex}", 0);
                if (this.AutoReplyVfeiOnError) SecondaryVfeiMsg?.Reply(Parent, 999, $"Exception caught when handling SecondaryIn S{trans.Secondary.Stream}F{trans.Secondary.Function}: {ex.Message}");
                //modError.HandleErrInfo(ex, Parent, true);
            }
            finally
            {
#if DEBUG
                this.WriteTrace($"Done handling S{trans.Secondary.Stream}F{trans.Secondary.Function} in HandleSecondaryIn.");
#endif
            }
        }


        public void SecondaryOut(SECSTransaction trans, long errorCode, string errorText)
        {
            try
            {
                if (errorCode != 0)
                {
                    if (AutoReplyVfeiOnError) SecondaryVfeiMsg?.Reply(Parent, errorCode, $"S{trans.Primary.Stream}F{trans.Primary.Function} send error: {errorText}");
                }
                else
                {
                    HandleSecondaryOut(trans);
                }
            }
            catch (Exception ex)
            {
                this.WriteTrace($"Exception caught while handling S{trans.Secondary.Stream}F{trans.Secondary.Function} in HandleSecondaryOut: {ex}", 0);
                if (this.AutoReplyVfeiOnError) SecondaryVfeiMsg?.Reply(Parent, 999, $"Exception caught when handling SecondaryOut S{trans.Secondary.Stream}F{trans.Secondary.Function}: {ex.Message}");
                //modError.HandleErrInfo(ex, Parent, true);
            }
            finally
            {
#if DEBUG
                this.WriteTrace($"Done handling S{trans.Secondary.Stream}F{trans.Secondary.Function} in HandleSecondaryOut.");
#endif
            }
        }

        public void SECSError(long errorCode, string errorText)
        {
            try
            {
                HandleSECSError(errorCode, errorText);
                if (this.AutoReplyVfeiOnError) SecondaryVfeiMsg?.Reply(Parent, errorCode, $"SECS Error occurred: {errorText}");
            }
            catch (Exception ex)
            {
                this.WriteTrace($"Exception caught while handling Secs Error event: {ex}", 0);
                modError.HandleErrInfo(ex, Parent, true);
            }
        }

        public void SECSWarning(long errorCode, string errorText)
        {
            try
            {
                HandleSECSWarning(errorCode, errorText);
            }
            catch (Exception ex)
            {
                this.WriteTrace($"Exception caught while handling Secs Warning event: {ex}", 0);
                modError.HandleErrInfo(ex, Parent, true);
            }
        }
    }
}

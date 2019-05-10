using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Runtime.InteropServices;

namespace T4Editor.Intellisense
{
    internal class T4CompletionCommandHandler : IOleCommandTarget
    {
        private IOleCommandTarget _nextCommandHandler;
        private ITextView _textView;
        private T4CompletionHandlerProvider _provider;
        private ICompletionSession session;

        internal T4CompletionCommandHandler(IVsTextView textViewAdapter, ITextView textView, T4CompletionHandlerProvider provider)
        {
            this._textView = textView;
            this._provider = provider;

            textViewAdapter.AddCommandFilter(this, out _nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (VsShellUtilities.IsInAutomationFunction(_provider.ServiceProvider))
            {
                return _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            uint commandID = nCmdID;
            char typedChar = char.MinValue;

            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN
                || nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB
                || (char.IsWhiteSpace(typedChar) || char.IsPunctuation(typedChar)))
            {
                if (session != null && !session.IsDismissed)
                {
                    if (session.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        session.Commit();
                        MoveCaret();
                        return VSConstants.S_OK;
                    }
                    else
                    {
                        session.Dismiss();
                    }
                }
            }

            int retVal = _nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            bool handled = false;

            if ((!typedChar.Equals(char.MinValue) && char.IsLetterOrDigit(typedChar)) || typedChar.Equals('<'))
            {
                if (session == null || session.IsDismissed)
                {
                    this.TriggerCompletion();
                    session.Filter();
                }
                else
                {
                    session.Filter();
                }
                handled = true;
            }
            else if (commandID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE
                || commandID == (uint)VSConstants.VSStd2KCmdID.DELETE)
            {
                if (session != null && !session.IsDismissed)
                    session.Filter();
                handled = true;
            }

            if (handled) return VSConstants.S_OK;

            return retVal;
        }

        private bool TriggerCompletion()
        {
            SnapshotPoint? caretPoint =
            _textView.Caret.Position.Point.GetPoint(
            textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);

            if (!caretPoint.HasValue)
            {
                return false;
            }

            session = _provider.CompletionBroker.CreateCompletionSession
         (_textView,
                caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
                true);

            session.Dismissed += this.OnSessionDismissed;
            session.Start();

            return true;
        }

        private void MoveCaret()
        {
            ITextViewLine textViewLine = _textView.Caret.ContainingTextViewLine;
            var xCoordinate = _textView.Caret.ContainingTextViewLine.Right;
            _textView.Caret.MoveTo(textViewLine, xCoordinate - 28.8);
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            session.Dismissed -= this.OnSessionDismissed;
            session = null;
        }
    }
}

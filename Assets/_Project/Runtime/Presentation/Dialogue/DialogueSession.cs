using System;
using System.Collections.Generic;

namespace FloatingIslandsRpg.Presentation.Dialogue
{
    public sealed class DialogueSession
    {
        private readonly List<string> _lines;
        private int _pageIndex;

        public bool IsActive { get; private set; }

        public string CurrentLine => IsActive ? _lines[_pageIndex] : null;

        public bool IsLastPage => _pageIndex >= _lines.Count - 1;

        public DialogueSession(IReadOnlyList<string> lines)
        {
            if (lines is null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            if (lines.Count == 0)
            {
                throw new ArgumentException("Dialogue must contain at least one line.", nameof(lines));
            }

            _lines = new List<string>(lines);
        }

        public void Start()
        {
            if (IsActive)
            {
                throw new InvalidOperationException("Dialogue session is already active.");
            }

            _pageIndex = 0;
            IsActive = true;
        }

        public bool Advance()
        {
            if (!IsActive)
            {
                throw new InvalidOperationException("Dialogue session is not active.");
            }

            if (IsLastPage)
            {
                IsActive = false;
                return false;
            }

            _pageIndex++;
            return true;
        }

        public void End()
        {
            IsActive = false;
        }
    }
}

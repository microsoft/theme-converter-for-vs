// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ThemeConverter.ColorCompiler
{
    internal abstract class OwnershipCollection<T> : ObservableCollection<T>
    {
        protected abstract void TakeOwnership(T item);
        protected abstract void LoseOwnership(T item);

        protected override void ClearItems()
        {
            List<T> removedElements = new List<T>(this);

            base.ClearItems();

            // it's important to only make this call
            // after the collection has been updated
            // so listeners see the current collection
            foreach (T element in removedElements)
            {
                LoseOwnership(element);
            }
        }

        protected override void InsertItem(int index, T item)
        {
            if (item == null)
            {
                // not an ArgumentNullException since this is not the public method originally called
                throw new InvalidOperationException("Collection item cannot be null");
            }

            base.InsertItem(index, item);

            // it's important to only make this call
            // after the collection has been updated
            // so listeners see the current collection
            TakeOwnership(item);
        }

        protected override void SetItem(int index, T item)
        {
            if (item == null)
            {
                // not an ArgumentNullException since this is not the public method originally called
                throw new InvalidOperationException("Collection item cannot be null");
            }

            T changedItem = this[index];

            base.SetItem(index, item);

            // it's important to only make this call
            // after the collection has been updated
            // so listeners see the current collection
            LoseOwnership(changedItem);
            TakeOwnership(item);
        }

        protected override void RemoveItem(int index)
        {
            T removedItem = this[index];

            base.RemoveItem(index);

            // it's important to only make this call
            // after the collection has been updated
            // so listeners see the current collection
            LoseOwnership(removedItem);
        }
    }
}

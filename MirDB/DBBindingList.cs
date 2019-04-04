﻿using System;
using System.ComponentModel;
using System.Reflection;

namespace MirDB
{
    public class DBBindingList<T> : BindingList<T> where T : DBObject, new()
    {
        private readonly Session Session;
        private readonly DBObject Parent;
        private readonly PropertyInfo Property;
        private readonly Association Link;

        public DBBindingList(DBObject parent, PropertyInfo property)
        {
            Session = parent.Collection.Session;
            Parent = parent;
            Property = property;
            Link = property.GetCustomAttribute<Association>();
        }

        protected override void OnAddingNew(AddingNewEventArgs e)
        {
            base.OnAddingNew(e);

            e.NewObject = Session.CreateObject<T>();
        }
        protected override void InsertItem(int index, T item)
        {
            if (Items.Contains(item)) return;

            base.InsertItem(index, item);

            CreateLink(item);
        }
        protected override void RemoveItem(int index)
        {
            T ob = Items[index];

            base.RemoveItem(index);

            RemoveLink(ob);
        }

        public void CreateLink(T ob)
        {
            if (ob == null || Link == null) return;

            PropertyInfo[] properties = ob.GetType().GetProperties();

            foreach (PropertyInfo p in properties)
            {
                Association obLink = p.GetCustomAttribute<Association>();

                if (obLink == null || obLink.Identity != Link.Identity || p.PropertyType != Property.DeclaringType || p == Property) continue;

                p.SetValue(ob, Parent);
                return;
            }


            throw new ArgumentException($"Unable to find Association {Parent.ThisType.Name}, Link: {Link.Identity ?? "Empty"} -> {ob.GetType()}");
        }
        public void RemoveLink(T ob)
        {
            if (ob == null || Link == null) return;

            PropertyInfo[] properties = ob.GetType().GetProperties();
            
            foreach (PropertyInfo p in properties)
            {
                Association obLink = p.GetCustomAttribute<Association>();

                if (obLink == null || obLink.Identity != Link.Identity || p.PropertyType != Property.DeclaringType || p == Property) continue;

                p.SetValue(ob, null);
                return;
            }
            
            throw new ArgumentException($"Unable to find Association {Parent.ThisType.Name}, Link: {Link.Identity ?? "Empty"} -> {ob.GetType()}");
        }
    }


}

//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CovidAppMVC.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Администраторы
    {
        public int Код_администратора { get; set; }
        public string Логин_администратора { get; set; }
        public string Пароль_администратора { get; set; }
        public int Код_сотрудника { get; set; }
    
        public virtual Сотрудники Сотрудники { get; set; }
    }
}

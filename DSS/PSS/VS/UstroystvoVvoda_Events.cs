﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonModel.StatisticsCollecting;
using CommonModel.RandomStreamProducing;
using CommonModel.Collections;
using CommonModel.Kernel;
using System.Runtime.Serialization;

namespace DSS.PSS.VS
{
    public partial class UstroystvoVvoda : Model
    {
        #region Описание событий УВД
        //Начало ввода данных заявки 
        public class Event_StartVvod_UVD : TimeModelEvent<UstroystvoVvoda>
        {
            #region Атрибуты события
            public VS.Zayavka Z;
            #endregion

            #region Алгоритм события
            protected override void HandleEvent(ModelEventArgs args)
            {
                DSS.Modeling.TraceString += "Заявка:" + Z.Num + " " + Model.Name + "</br>"; //Выводим сообщение в трассирову о совершённом событии
                Model.KVZ.Value++; // Собираем статистику по количеству вошедших заявок


                //Если устройство ввода свободно
                if (!Model.Zanyatost.Value)
                {
                    //Планировать завершение ввода данных 
                    var ev = new Event_FinishVvod_UVD();
                    ev.Z = Z; //Передаём заявку в планируемое событие
                    double dt = Model.KOEF * Z.RazmerVvod; //Назначаем время через которое произойдёт событие
                    Model.PlanEvent(ev, dt); //Планируем совершение события
                    DSS.Modeling.TraceString += "Заявка:" + Z.Num + " " + Model.Name + "</br>"; //Выводим сообщение в трассировку о планируемом событии
                    Model.ZayNum = Z.Num; //Регистрируем номер заявки
                    Model.Zanyatost.Ref = Z; //Занимаем устройство заявкой
                }
                //Если устройство ввода занято
                else
                {
                    //Добавить заявку в очередь
                    var zayrec = new QRec(); //Создаём запись
                    zayrec.Z = Z; //Передаём заявку в запись
                    Model.Que.Add(zayrec); //Добавляем запись в очередь
                }
            }
            #endregion
        }
        //Завершение ввода данных заявки 
        public class Event_FinishVvod_UVD : TimeModelEvent<UstroystvoVvoda>
        {
            #region Атрибуты события
            public VS.Zayavka Z;
            #endregion

            #region Алгоритм события
            protected override void HandleEvent(ModelEventArgs args)
            {
                DSS.Modeling.TraceString += "Заявка:" + Z.Num + " " + Model.Name + "</br>"; //Выводим сообщение в трассирову о совершённом событии
                Model.KOZ.Value++; //Сбор статистики по количеству обработанных заявок

                //Вызов события конца ввода данных
                var ev = new VU.Event_FinishVvod_VU();
                ev.Z = Z; //Передаём заявку в планируемое событие
                Model.parentVU.RunEventHandlerNow(ev); //Вызываем события в родительском классе

                //Если есть заявки в очереди
                if (Model.Que.Count != 0)
                {
                    //Извлечь заявкку из очереди 
                    var rec = new QRec(); //Создаём запись
                    rec = Model.Que.Pop(); //Извлекаем заявку

                    //Запланировать событие завершения ввода данных
                    var evf = new Event_FinishVvod_UVD();
                    double dt = Model.KOEF * rec.Z.RazmerVvod; //Назначаем время через которое произойдёт событие
                    evf.Z = rec.Z; //Передаём заявку в планируемое событие
                    Model.PlanEvent(evf, dt); //Планируем совершение события
                    DSS.Modeling.TraceString += "Заявка:" + rec.Z.Num + " " + Model.Name + "</br>"; //Выводим сообщение в трассировку о планируемом событии
                    Model.ZayNum = rec.Z.Num; //Регестрируем номер заявки
                    Model.Zanyatost.Ref = rec.Z; //Занимаем устройство заявкой
                }
                //Освободить устройство
                else
                {
                    Model.ZayNum = 0; //Если устройство свободно, номер обрабатываемой заявки нулевой
                    Model.Zanyatost.Ref = null; //Устройство не занято никакой заявкой
                }
            }
            #endregion
        }
        #endregion
    }
}
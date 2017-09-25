﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxModel
{
    public class Singleton
    {
        private static Singleton instance = null;

        private Singleton() {  }

        public ListePatientDataGrid Patient { get; set; }
        public Patient PatientSingleton { get; set; }
        private static bool robotError = false;
        public static ConfigProgramme ConfigProg { get; set; }
        public Admin Admin { get; set; }
        public static bool IsCalibre { get; set; }
        public static bool CriticalError { get; set; }
        public static bool UniBi { get; set; }
        public static bool MainGaucheX { get; set; }
        public static double CalibrX { get; set; }
        public static double CalibrY { get; set; }

        public static void identification()
        {
            if (instance == null)
            {
                instance = new Singleton();
            }
        }

        public static void identificationAdmin()
        {
            if (instance == null)
            {
                instance = new Singleton();
            }
        }

        public static void logOffPatient()
        {
            instance.Patient = null;
            instance.PatientSingleton = null;

        }

        public static void logOff()
        {
            instance = null;

        }

        public static bool GetRobotError()
        {
            return robotError;
        }

        public static void ChangeErrorStatu(bool newStatu)
        {
            robotError = newStatu;
        }

        public static Singleton getInstance()
        {
            return instance;
        }
    }
}

namespace PIPMUNI_ARG
{
    public class ProjectGlobals
    {
        //==============================================
        //                  Standard
        //
        //----------------------------------------------

        public static string defaultPassword = "Change@123";

        public static int maxRowsBeforeSearch = 0;

        static public DateTime MinValidDate = new DateTime(1964, 01, 01);
        static public DateTime MaxValidDate = new DateTime(2064, 01, 01);
        static public string MinValidDateString = "1964/01/01";
        static public string MaxValidDateString = "2064/01/01";

        //==============================================
        //              Project Specifics
        //
        //----------------------------------------------
        public static string defaultLatitude = "-31.2516"; // ba -34.61
        public static string defaultLongitude = "-61.4917"; // ba -58.38

        public static int MaxCellStringLength = 32767;

        public const string RoleAdmin = "Administrator";
        public const string RoleDireccion = "Direccion";
        public const string RoleOperacion = "Operacion";
        public const string RoleConsulta = "Consulta";
        public const string registeredRoles = "Administrator,Direccion,Operacion,Consulta";


        static public class AdditionStage
        {
            public const int Iniciada = 1;
            public const int EnProceso = 2;
            public const int Aprobada = 3;
        }

        static public class ContractStage
        {
            public const int Iniciar = 1;
            public const int Ejecucion = 2;
            public const int Finalizada = 3;
            public const int Rescindida = 4;
        }

        static public class PaymentStage
        {
            public const int AreaTecnica = 1;
            public const int AreaContable = 3;
            public const int Devengado = 4;
            public const int Finalizado = 5;
        }
        static public class ProjectStage
        {
            public const int Iniciar = 1;
            public const int Ejecucion = 2;
            public const int Finalizada = 3;
            public const int Rescindida = 4;
        }
        static public class VarianteMotivo
        {
            public const int Ampliacion = 1;
            public const int Reduccion = 2;
            public const int Variante = 3;
            public const int Demasias = 4;
        }


    }
}

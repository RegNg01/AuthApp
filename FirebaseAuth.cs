using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace WpfApp1
{
    internal class FirebaseAuth
    {
        public IFirebaseClient client;
        public IFirebaseConfig config;
        public FirebaseAuth()
        {
              
            config = new FirebaseConfig
            {
                AuthSecret = "ugtrpVJNfiK4987ZumN9BE0yyWcOBTjRQH6lTYOa",
                BasePath = "https://authapp-60a49-default-rtdb.europe-west1.firebasedatabase.app/"
            }; 
            client = new FirebaseClient(config);
             

    }
         
 

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace System
{

    public partial class DSAProvider
    {
        static partial void GetPrivateProvider(ref DSACryptoServiceProvider provider);

        public static DSACryptoServiceProvider GetProvider(string xmlKey)
        {
            DSACryptoServiceProvider res = new DSACryptoServiceProvider();
            res.FromXmlString(xmlKey);
            return res;
        }


        public static DSACryptoServiceProvider PublicProvider
        {
            get
            {
                return GetProvider(@"
                <DSAKeyValue>
                    <P>ouFdQeGqPL3VS7aOVYtIVKUzb/PQM4f3GKLecWIGSWYMob/6Y8s3zd93l0VJMuju+yObuZ+o9nc29QjsP4xgYl8qP/+MQdP/FuKj/v/vmN9401vO1PoNGRp8j+pVdUuMe8KOzEqS8PxhiHO/ib+yAQAxYrJPb9ErE8YjegTqMjs=</P> 
                    <Q>gY7vlY7S3YOWm63/a2YPHIfdTXM=</Q> 
                    <G>ELvXVSTH/YE9Rp5tyiSnEMObyRC2n3Ie6lxdxF1O52pUzqiqqfeA2mKuwZNdk4z6fHQ4rWScLl9S1Q1LCK3Pz5e6ONSKHO+qKuVfPq04Tqv3oDQRCzIJL4jxAenTBiW+jfLu0FjeNb4yfvcYeq3OAncgRDnNJN0x4dcMuteJFNw=</G> 
                    <Y>Rm335+1w6jWadZOquBpJT7OCXF8ovv+tLQQjBtQmCohzts1oHMMOoteQr814orSQBThw/xl0/ibe7w8Pb3HGcRf4rO2ljReiG44ywVHW6ejuTGsTBvol9JSYnBhlT7aizYj8WGlRIlqpEed0InnZfay4+hj+Ha+9BuqxXCUCbRE=</Y> 
                    <J>AAAAAUHXpZQ3KgbQhPIhKUr82mw6EIZPXe+wBECS1ScRoY4zJLMD168nRM/tkuREdL//Zl5KF3+2APeNpOLWuMeczvSmtiNBkcUyTifXGr5sN/xtFVF+FqMIQtSls5XXSBbwbtU4qXD3admP2wi2Xg==</J> 
                    <Seed>q9wHFTSRXcK65NX2eMzWENYfPGI=</Seed> 
                    <PgenCounter>Ng==</PgenCounter> 
                </DSAKeyValue>"
                );
            }
        }
    }
}

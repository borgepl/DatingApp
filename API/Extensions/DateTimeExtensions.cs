using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int Age(this DateTime dateOfBirth) {
            if ( DateTime.Today.Month < dateOfBirth.Month ||
	             DateTime.Today.Month == dateOfBirth.Month &&
	             DateTime.Today.Day < dateOfBirth.Day ) 
            {
                return DateTime.Today.Year - dateOfBirth.Year - 1;
	        } 
	        return DateTime.Today.Year - dateOfBirth.Year; 
        }
    }
}
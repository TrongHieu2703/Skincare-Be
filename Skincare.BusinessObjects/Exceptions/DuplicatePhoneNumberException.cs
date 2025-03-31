using System;

namespace Skincare.BusinessObjects.Exceptions
{
    public class DuplicatePhoneNumberException : Exception
    {
        public DuplicatePhoneNumberException() : base("The phone number already exists.")
        {
        }

        public DuplicatePhoneNumberException(string message) : base(message)
        {
        }

        public DuplicatePhoneNumberException(string message, Exception inner) : base(message, inner)
        {
        }
    }
} 
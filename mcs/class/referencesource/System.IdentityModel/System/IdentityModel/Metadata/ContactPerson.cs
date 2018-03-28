//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines the contact person class.
    /// </summary>
    public class ContactPerson
    {
        ContactType _type = ContactType.Unspecified;
        string _company;
        string _givenName;
        string _surname;
        Collection<string> _emailAddresses = new Collection<string>();
        Collection<string> _telephoneNumbers = new Collection<string>();

        /// <summary>
        /// Empty constructor for contact person.
        /// </summary>
        public ContactPerson()
        {
        }

        /// <summary>
        /// Creates a contact person object with the contact type.
        /// </summary>
        /// <param name="contactType">The <see cref="ContactType"/> for this object.</param>
        public ContactPerson(ContactType contactType)
        {
            _type = contactType;
        }

        /// <summary>
        /// Gets or sets the company name.
        /// </summary>
        public string Company
        {
            get { return _company; }
            set { _company = value; }
        }

        /// <summary>
        /// Gets the email address collection.
        /// </summary>
        public ICollection<string> EmailAddresses
        {
            get { return _emailAddresses; }
        }

        /// <summary>
        /// Gets or sets the given name.
        /// </summary>
        public string GivenName
        {
            get { return _givenName; }
            set { _givenName = value; }
        }

        /// <summary>
        /// Gets or sets the surname.
        /// </summary>
        public string Surname
        {
            get { return _surname; }
            set { _surname = value; }
        }

        /// <summary>
        /// Gets the collection of telephone numbers.
        /// </summary>
        public ICollection<string> TelephoneNumbers
        {
            get { return _telephoneNumbers; }
        }

        /// <summary>
        /// Gets or sets the contact type.
        /// </summary>
        public ContactType Type
        {
            get { return _type; }
            set { _type = value; }
        }

    }
}

using MixMedia.MCP.LexOffice.Models;
using System;
using System.Collections.Generic;

namespace MixMedia.MCP.LexOffice.Tools
{
    // Helper DTOs for parameters to make the method signature cleaner

    public class CompanyPayload
    {
        public required string Name { get; set; }
        public string? TaxNumber { get; set; }
        public string? VatRegistrationId { get; set; }
        public bool? AllowTaxFreeInvoices { get; set; }
        public ContactPersonPayload? ContactPerson { get; set; } // Max one for creation as per API docs
    }

    public class PersonPayload // For the main contact type if it's a person
    {
        public string? Salutation { get; set; }
        public string? FirstName { get; set; }
        public required string LastName { get; set; }
    }

    public class ContactPersonPayload // For a contact person within a company
    {
        public string? Salutation { get; set; }
        public string? FirstName { get; set; }
        public required string LastName { get; set; }
        public bool Primary { get; set; } = true; // Defaults to true, API doc says optional
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class AddressPayload
    {
        public string? Supplement { get; set; }
        public string? Street { get; set; }
        public string? Zip { get; set; }
        public string? City { get; set; }
        public required string CountryCode { get; set; }
    }

    public class EmailAddressesPayload
    {
        public List<string>? Business { get; set; } // Max 1 item
        public List<string>? Office { get; set; }   // Max 1 item
        public List<string>? Private { get; set; }  // Max 1 item
        public List<string>? Other { get; set; }    // Max 1 item
    }

    public class PhoneNumbersPayload
    {
        public List<string>? Business { get; set; } // Max 1 item
        public List<string>? Office { get; set; }   // Max 1 item
        public List<string>? Mobile { get; set; }   // Max 1 item
        public List<string>? Private { get; set; }  // Max 1 item
        public List<string>? Fax { get; set; }      // Max 1 item
        public List<string>? Other { get; set; }    // Max 1 item
    }

    public class XRechnungPayload
    {
        public string? BuyerReference { get; set; }
        public string? VendorNumberAtCustomer { get; set; }
    }

    public class CreateContactTool
    {
        public Contact PrepareContactPayload(
            bool isCustomer,
            bool isVendor,
            CompanyPayload? companyPayload,
            PersonPayload? personPayload,
            AddressPayload? billingAddressPayload,
            AddressPayload? shippingAddressPayload,
            EmailAddressesPayload? emailAddressesPayload,
            PhoneNumbersPayload? phoneNumbersPayload,
            string? note,
            XRechnungPayload? xRechnungPayload,
            bool archived = false)
        {
            // --- Validation ---
            if (!isCustomer && !isVendor)
            {
                throw new ArgumentException("At least one role (customer or vendor) must be specified.");
            }

            if (companyPayload == null && personPayload == null)
            {
                throw new ArgumentException("Either company details or person details must be provided for the contact type.");
            }

            if (companyPayload != null && personPayload != null)
            {
                throw new ArgumentException("Provide either company details or person details for the contact type, not both.");
            }

            var contact = new Contact
            {
                Version = 0, // Mandatory as per API documentation
                Archived = archived,
                Note = note
            };

            // --- Roles ---
            contact.Roles = new ContactRoles();
            if (isCustomer)
            {
                contact.Roles.Customer = new ContactRoleCustomer(); // Number is determined by lexoffice
            }
            if (isVendor)
            {
                contact.Roles.Vendor = new ContactRoleVendor(); // Number is determined by lexoffice
            }

            // --- Company or Person ---
            if (companyPayload != null)
            {
                if (string.IsNullOrWhiteSpace(companyPayload.Name))
                {
                    throw new ArgumentException("Company name is mandatory if company details are provided.");
                }
                contact.Company = new ContactCompany
                {
                    Name = companyPayload.Name,
                    TaxNumber = companyPayload.TaxNumber,
                    VatRegistrationId = companyPayload.VatRegistrationId,
                    AllowTaxFreeInvoices = companyPayload.AllowTaxFreeInvoices,
                    ContactPersons = new List<ContactPersonDetails>() // Initialize to ensure it's not null
                };

                if (companyPayload.ContactPerson != null)
                {
                    if (string.IsNullOrWhiteSpace(companyPayload.ContactPerson.LastName))
                    {
                        throw new ArgumentException("Contact person's last name is mandatory if a contact person is provided for a company.");
                    }
                    // API: "A maximum of one contact person can be created or updated via the API."
                    contact.Company.ContactPersons.Add(new ContactPersonDetails
                    {
                        Salutation = companyPayload.ContactPerson.Salutation,
                        FirstName = companyPayload.ContactPerson.FirstName,
                        LastName = companyPayload.ContactPerson.LastName,
                        Primary = companyPayload.ContactPerson.Primary,
                        EmailAddress = companyPayload.ContactPerson.EmailAddress,
                        PhoneNumber = companyPayload.ContactPerson.PhoneNumber
                    });
                }
            }
            else if (personPayload != null)
            {
                if (string.IsNullOrWhiteSpace(personPayload.LastName))
                {
                    throw new ArgumentException("Person's last name is mandatory if person details are provided.");
                }
                contact.Person = new ContactPerson
                {
                    Salutation = personPayload.Salutation ?? string.Empty, // Model expects non-null string
                    FirstName = personPayload.FirstName ?? string.Empty, // Model expects non-null string
                    LastName = personPayload.LastName
                    // Other fields like Primary, EmailAddress, PhoneNumber are not part of the top-level "person" object in API docs.
                    // The ContactPerson model is also used for Company.ContactPersons where those fields are relevant.
                };
            }

            // --- Addresses ---
            // Model initializes Addresses, Billing, and Shipping to new instances.
            if (billingAddressPayload != null)
            {
                if (string.IsNullOrWhiteSpace(billingAddressPayload.CountryCode))
                {
                    throw new ArgumentException("Country code is mandatory for billing address.");
                }
                // API: "A maximum of one billing... address can be created"
                contact.Addresses.Billing.Add(new ContactAddress
                {
                    Supplement = billingAddressPayload.Supplement,
                    Street = billingAddressPayload.Street,
                    Zip = billingAddressPayload.Zip,
                    City = billingAddressPayload.City,
                    CountryCode = billingAddressPayload.CountryCode
                });
            }

            if (shippingAddressPayload != null)
            {
                if (string.IsNullOrWhiteSpace(shippingAddressPayload.CountryCode))
                {
                    throw new ArgumentException("Country code is mandatory for shipping address.");
                }
                // API: "A maximum of one ... shipping address can be created"
                contact.Addresses.Shipping.Add(new ContactAddress
                {
                    Supplement = shippingAddressPayload.Supplement,
                    Street = shippingAddressPayload.Street,
                    Zip = shippingAddressPayload.Zip,
                    City = shippingAddressPayload.City,
                    CountryCode = shippingAddressPayload.CountryCode
                });
            }

            // --- Email Addresses ---
            // Model initializes EmailAddresses and its lists to new instances.
            if (emailAddressesPayload != null)
            {
                // API: "A maximum of one email address per type"
                if (emailAddressesPayload.Business != null)
                {
                    if (emailAddressesPayload.Business.Count > 1) throw new ArgumentException("Maximum of one business email address is allowed.");
                    contact.EmailAddresses.Business = emailAddressesPayload.Business;
                }
                if (emailAddressesPayload.Office != null)
                {
                    if (emailAddressesPayload.Office.Count > 1) throw new ArgumentException("Maximum of one office email address is allowed.");
                    contact.EmailAddresses.Office = emailAddressesPayload.Office;
                }
                if (emailAddressesPayload.Private != null)
                {
                    if (emailAddressesPayload.Private.Count > 1) throw new ArgumentException("Maximum of one private email address is allowed.");
                    contact.EmailAddresses.Private = emailAddressesPayload.Private;
                }
                if (emailAddressesPayload.Other != null)
                {
                    if (emailAddressesPayload.Other.Count > 1) throw new ArgumentException("Maximum of one other email address is allowed.");
                    contact.EmailAddresses.Other = emailAddressesPayload.Other;
                }
            }

            // --- Phone Numbers ---
            // Model initializes PhoneNumbers and its lists to new instances.
            if (phoneNumbersPayload != null)
            {
                // API: "A maximum of one phone number per type"
                if (phoneNumbersPayload.Business != null)
                {
                    if (phoneNumbersPayload.Business.Count > 1) throw new ArgumentException("Maximum of one business phone number is allowed.");
                    contact.PhoneNumbers.Business = phoneNumbersPayload.Business;
                }
                if (phoneNumbersPayload.Office != null)
                {
                    if (phoneNumbersPayload.Office.Count > 1) throw new ArgumentException("Maximum of one office phone number is allowed.");
                    contact.PhoneNumbers.Office = phoneNumbersPayload.Office;
                }
                if (phoneNumbersPayload.Mobile != null)
                {
                    if (phoneNumbersPayload.Mobile.Count > 1) throw new ArgumentException("Maximum of one mobile phone number is allowed.");
                    contact.PhoneNumbers.Mobile = phoneNumbersPayload.Mobile;
                }
                if (phoneNumbersPayload.Private != null)
                {
                    if (phoneNumbersPayload.Private.Count > 1) throw new ArgumentException("Maximum of one private phone number is allowed.");
                    contact.PhoneNumbers.Private = phoneNumbersPayload.Private;
                }
                if (phoneNumbersPayload.Fax != null)
                {
                    if (phoneNumbersPayload.Fax.Count > 1) throw new ArgumentException("Maximum of one fax phone number is allowed.");
                    contact.PhoneNumbers.Fax = phoneNumbersPayload.Fax;
                }
                if (phoneNumbersPayload.Other != null)
                {
                    if (phoneNumbersPayload.Other.Count > 1) throw new ArgumentException("Maximum of one other phone number is allowed.");
                    contact.PhoneNumbers.Other = phoneNumbersPayload.Other;
                }
            }

            // --- XRechnung ---
            if (xRechnungPayload != null)
            {
                contact.XRechnung = new ContactXRechnung
                {
                    BuyerReference = xRechnungPayload.BuyerReference,
                    VendorNumberAtCustomer = xRechnungPayload.VendorNumberAtCustomer
                };
            }

            return contact;
        }
    }
}

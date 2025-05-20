using System;
using System.Collections.Generic;

namespace MixMedia.MCP.LexOffice.Models;

public class Contact
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public int Version { get; set; }
    public ContactRoles Roles { get; set; } = new();
    public ContactCompany? Company { get; set; }
    public ContactPerson? Person { get; set; }
    public ContactAddresses Addresses { get; set; } = new();
    public ContactXRechnung? XRechnung { get; set; }
    public ContactEmailAddresses EmailAddresses { get; set; } = new();
    public ContactPhoneNumbers PhoneNumbers { get; set; } = new();
    public string? Note { get; set; }
    public bool Archived { get; set; }
}

public class ContactRoles
{
    public ContactRoleCustomer? Customer { get; set; }
    public ContactRoleVendor? Vendor { get; set; }
}

public class ContactRoleCustomer
{
    public int Number { get; set; }
}

public class ContactRoleVendor
{
    public int Number { get; set; }
}

public class ContactCompany
{
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? VatRegistrationId { get; set; }
    public bool? AllowTaxFreeInvoices { get; set; }
    public List<ContactPersonDetails> ContactPersons { get; set; } = new();
}

public class ContactPerson
{
    public string Salutation { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool Primary { get; set; }
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
}

public class ContactPersonDetails
{
    public string Salutation { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool Primary { get; set; }
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
}

public class ContactAddresses
{
    public List<ContactAddress> Billing { get; set; } = new();
    public List<ContactAddress> Shipping { get; set; } = new();
}

public class ContactAddress
{
    public string? Supplement { get; set; }
    public string? Street { get; set; }
    public string? Zip { get; set; }
    public string? City { get; set; }
    public string? CountryCode { get; set; }
}

public class ContactXRechnung
{
    public string? BuyerReference { get; set; }
    public string? VendorNumberAtCustomer { get; set; }
}

public class ContactEmailAddresses
{
    public List<string> Business { get; set; } = new();
    public List<string> Office { get; set; } = new();
    public List<string> Private { get; set; } = new();
    public List<string> Other { get; set; } = new();
}

public class ContactPhoneNumbers
{
    public List<string> Business { get; set; } = new();
    public List<string> Office { get; set; } = new();
    public List<string> Mobile { get; set; } = new();
    public List<string> Private { get; set; } = new();
    public List<string> Fax { get; set; } = new();
    public List<string> Other { get; set; } = new();
}

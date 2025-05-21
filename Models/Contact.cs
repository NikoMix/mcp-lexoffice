using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models;

public class Contact
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("organizationId")]
    public Guid OrganizationId { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("roles")]
    public ContactRoles Roles { get; set; } = new();

    [JsonPropertyName("company")]
    public ContactCompany? Company { get; set; }

    [JsonPropertyName("person")]
    public ContactPerson? Person { get; set; }

    [JsonPropertyName("addresses")]
    public ContactAddresses Addresses { get; set; } = new();

    [JsonPropertyName("xRechnung")]
    public ContactXRechnung? XRechnung { get; set; }

    [JsonPropertyName("emailAddresses")]
    public ContactEmailAddresses EmailAddresses { get; set; } = new();

    [JsonPropertyName("phoneNumbers")]
    public ContactPhoneNumbers PhoneNumbers { get; set; } = new();

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("archived")]
    public bool Archived { get; set; }
}

public class ContactRoles
{
    [JsonPropertyName("customer")]
    public ContactRoleCustomer? Customer { get; set; }

    [JsonPropertyName("vendor")]
    public ContactRoleVendor? Vendor { get; set; }
}

public class ContactRoleCustomer
{
    [JsonPropertyName("number")]
    public int Number { get; set; } // This is usually read-only from LexOffice
}

public class ContactRoleVendor
{
    [JsonPropertyName("number")]
    public int Number { get; set; } // This is usually read-only from LexOffice
}

public class ContactCompany
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("taxNumber")]
    public string? TaxNumber { get; set; }

    [JsonPropertyName("vatRegistrationId")]
    public string? VatRegistrationId { get; set; }

    [JsonPropertyName("allowTaxFreeInvoices")]
    public bool? AllowTaxFreeInvoices { get; set; }

    [JsonPropertyName("contactPersons")]
    public List<ContactPersonDetails> ContactPersons { get; set; } = new();
}

public class ContactPerson // This class seems to be for the top-level 'person' object
{
    [JsonPropertyName("salutation")]
    public string Salutation { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    // These fields are not typically part of the main "person" object for contact creation,
    // but rather for "contactPersons" within a "company".
    // If they are indeed for the top-level person, ensure API supports it.
    // For now, I'm assuming they are as per your original model.
    [JsonPropertyName("primary")]
    public bool Primary { get; set; }

    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }
}

public class ContactPersonDetails // This class is for 'contactPersons' within 'company'
{
    [JsonPropertyName("salutation")]
    public string Salutation { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("primary")]
    public bool Primary { get; set; }

    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }
}

public class ContactAddresses
{
    [JsonPropertyName("billing")]
    public List<ContactAddress> Billing { get; set; } = new();

    [JsonPropertyName("shipping")]
    public List<ContactAddress> Shipping { get; set; } = new();
}

public class ContactAddress
{
    [JsonPropertyName("supplement")]
    public string? Supplement { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("zip")]
    public string? Zip { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; } // Mandatory for creation if address is provided
}

public class ContactXRechnung
{
    [JsonPropertyName("buyerReference")]
    public string? BuyerReference { get; set; }

    [JsonPropertyName("vendorNumberAtCustomer")]
    public string? VendorNumberAtCustomer { get; set; }
}

public class ContactEmailAddresses
{
    [JsonPropertyName("business")]
    public List<string> Business { get; set; } = new(); // Max 1 for creation

    [JsonPropertyName("office")]
    public List<string> Office { get; set; } = new();   // Max 1 for creation

    [JsonPropertyName("private")]
    public List<string> Private { get; set; } = new();  // Max 1 for creation

    [JsonPropertyName("other")]
    public List<string> Other { get; set; } = new();    // Max 1 for creation
}

public class ContactPhoneNumbers
{
    [JsonPropertyName("business")]
    public List<string> Business { get; set; } = new(); // Max 1 for creation

    [JsonPropertyName("office")]
    public List<string> Office { get; set; } = new();   // Max 1 for creation

    [JsonPropertyName("mobile")]
    public List<string> Mobile { get; set; } = new();   // Max 1 for creation

    [JsonPropertyName("private")]
    public List<string> Private { get; set; } = new();  // Max 1 for creation

    [JsonPropertyName("fax")]
    public List<string> Fax { get; set; } = new();      // Max 1 for creation

    [JsonPropertyName("other")]
    public List<string> Other { get; set; } = new();    // Max 1 for creation
}

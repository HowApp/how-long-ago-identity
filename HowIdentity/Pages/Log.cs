// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace HowIdentity.Pages;

using Microsoft.AspNetCore.Identity;

internal static class Log
{
    private static readonly Action<ILogger, string?, Exception?> _invalidId = LoggerMessage.Define<string?>(
        LogLevel.Error,
        EventIds.InvalidId,
        "Invalid id {Id}");

    public static void InvalidId(this ILogger logger, string? id)
    {
        _invalidId(logger, id, null);
    }

    private static readonly Action<ILogger, string?, Exception?> _invalidBackchannelLoginId =
        LoggerMessage.Define<string?>(
            LogLevel.Warning,
            EventIds.InvalidBackchannelLoginId,
            "Invalid backchannel login id {Id}");

    public static void InvalidBackchannelLoginId(this ILogger logger, string? id)
    {
        _invalidBackchannelLoginId(logger, id, null);
    }

    private static Action<ILogger, IEnumerable<string>, Exception?> _externalClaims =
        LoggerMessage.Define<IEnumerable<string>>(
            LogLevel.Debug,
            EventIds.ExternalClaims,
            "External claims: {Claims}");

    public static void ExternalClaims(this ILogger logger, IEnumerable<string> claims)
    {
        _externalClaims(logger, claims, null);
    }

    private static Action<ILogger, string, Exception?> _noMatchingBackchannelLoginRequest =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            EventIds.NoMatchingBackchannelLoginRequest,
            "No backchannel login request matching id: {Id}");

    public static void NoMatchingBackchannelLoginRequest(this ILogger logger, string id)
    {
        _noMatchingBackchannelLoginRequest(logger, id, null);
    }

    private static Action<ILogger, string, Exception?> _noConsentMatchingRequest = LoggerMessage.Define<string>(
        LogLevel.Error,
        EventIds.NoConsentMatchingRequest,
        "No consent request matching request: {ReturnUrl}");

    public static void NoConsentMatchingRequest(this ILogger logger, string returnUrl)
    {
        _noConsentMatchingRequest(logger, returnUrl, null);
    }

    private static Action<ILogger, string, Exception?> _identityErrorRequest(int eventId, string error) =>
        LoggerMessage.Define<string>(
            LogLevel.Error,
            eventId,
            $"Registration Error {error}"
            );

    public static void IdentityCreateError(this ILogger logger, IEnumerable<IdentityError> errors)
    {
        var error = string.Join("\n", errors.Select(error => $"Code: {error.Code}, Description: {error.Description};\n"));
        _identityErrorRequest(EventIds.CreateUserError, error);
    }
    
    public static void IdentityRoleError(this ILogger logger, IEnumerable<IdentityError> errors)
    {
        var error = string.Join("\n", errors.Select(error => $"Code: {error.Code}, Description: {error.Description};\n"));
        _identityErrorRequest(EventIds.AddToRoleError, error);
    }
}

internal static class EventIds
{
    private const int UIEventsStart = 10000;

    //////////////////////////////
    // Consent
    //////////////////////////////
    private const int ConsentEventsStart = UIEventsStart + 1000;
    public const int InvalidId = ConsentEventsStart + 0;
    public const int NoConsentMatchingRequest = ConsentEventsStart + 1;

    //////////////////////////////
    // External Login
    //////////////////////////////
    private const int ExternalLoginEventsStart = UIEventsStart + 2000;
    public const int ExternalClaims = ExternalLoginEventsStart + 0;

    //////////////////////////////
    // CIBA
    //////////////////////////////
    private const int CibaEventsStart = UIEventsStart + 3000;
    public const int InvalidBackchannelLoginId = CibaEventsStart + 0;
    public const int NoMatchingBackchannelLoginRequest = CibaEventsStart + 1;
    
    //////////////////////////////
    // Registration
    //////////////////////////////
    private const int RegistrationEventsStart = UIEventsStart + 4000;
    public const int CreateUserError = RegistrationEventsStart + 0;
    public const int AddToRoleError = CreateUserError + 1;
}
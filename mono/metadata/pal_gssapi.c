// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "config.h"
#include <stdint.h>
#include <glib.h>
#include <mono/metadata/pal_gssapi.h>

#if HAVE_GSS_GSS_H
#include <GSS/GSS.h>
#else
#if HAVE_GSSAPI_GSSAPI_H
#include <gssapi/gssapi.h>
#include <gssapi/gssapi_krb5.h>
#else
#include <gssapi/gssapi_ext.h>
#include <gssapi/gssapi_krb5.h>
#endif
#endif

#define ARRAYSIZE(arr) (sizeof(arr) / sizeof((arr)[0]))


#if !defined(GSS_SPNEGO_MECHANISM)
static char gss_spnego_oid_value[] = "\x2b\x06\x01\x05\x05\x02"; // Binary representation of SPNEGO Oid (RFC 4178)
static gss_OID_desc gss_mech_spnego_OID_desc = {.length = ARRAYSIZE(gss_spnego_oid_value) - 1,
                                                .elements = (void*)gss_spnego_oid_value};
static char gss_ntlm_oid_value[] =
    "\x2b\x06\x01\x04\x01\x82\x37\x02\x02\x0a"; // Binary representation of NTLM OID
                                                // (https://msdn.microsoft.com/en-us/library/cc236636.aspx)
static gss_OID_desc gss_mech_ntlm_OID_desc = {.length = ARRAYSIZE(gss_ntlm_oid_value) - 1,
                                              .elements = (void*)gss_ntlm_oid_value};
#endif

// transfers ownership of the underlying data from gssBuffer to PAL_GssBuffer
static void NetSecurityNative_MoveBuffer(gss_buffer_t gssBuffer, struct PAL_GssBuffer* targetBuffer)
{
    g_assert(gssBuffer != 0);
    g_assert(targetBuffer != 0);

    targetBuffer->length = (uint64_t)gssBuffer->length;
    targetBuffer->data = (uint8_t*)gssBuffer->value;
}

static uint32_t NetSecurityNative_AcquireCredSpNego(uint32_t* minorStatus,
                                                    GssName* desiredName,
                                                    gss_cred_usage_t credUsage,
                                                    GssCredId** outputCredHandle)
{
    g_assert(minorStatus != 0);
    g_assert(desiredName != 0);
    g_assert(outputCredHandle != 0);
    g_assert(*outputCredHandle == 0);

#if defined(GSS_SPNEGO_MECHANISM)
    gss_OID_set_desc gss_mech_spnego_OID_set_desc = {.count = 1, .elements = GSS_SPNEGO_MECHANISM};
#else
    gss_OID_set_desc gss_mech_spnego_OID_set_desc = {.count = 1, .elements = &gss_mech_spnego_OID_desc};
#endif
    uint32_t majorStatus = gss_acquire_cred(
        minorStatus, desiredName, 0, &gss_mech_spnego_OID_set_desc, credUsage, outputCredHandle, 0, 0);

    // call gss_set_cred_option with GSS_KRB5_CRED_NO_CI_FLAGS_X to support Kerberos Sign Only option from *nix client against a windows server
#if defined(GSS_KRB5_CRED_NO_CI_FLAGS_X)
    if (majorStatus == GSS_S_COMPLETE)
    {
        GssBuffer emptyBuffer = GSS_C_EMPTY_BUFFER;
        majorStatus = gss_set_cred_option(minorStatus, outputCredHandle, GSS_KRB5_CRED_NO_CI_FLAGS_X, &emptyBuffer);
    }
#endif

    return majorStatus;
}

uint32_t
NetSecurityNative_InitiateCredSpNego(uint32_t* minorStatus, GssName* desiredName, GssCredId** outputCredHandle)
{
    return NetSecurityNative_AcquireCredSpNego(minorStatus, desiredName, GSS_C_INITIATE, outputCredHandle);
}

uint32_t NetSecurityNative_DeleteSecContext(uint32_t* minorStatus, GssCtxId** contextHandle)
{
    g_assert(minorStatus != 0);
    g_assert(contextHandle != 0);

    return gss_delete_sec_context(minorStatus, contextHandle, GSS_C_NO_BUFFER);
}

static uint32_t NetSecurityNative_DisplayStatus(uint32_t* minorStatus,
                                                uint32_t statusValue,
                                                int statusType,
                                                struct PAL_GssBuffer* outBuffer)
{
    g_assert(minorStatus != 0);
    g_assert(outBuffer != 0);

    uint32_t messageContext;
    GssBuffer gssBuffer = {.length = 0, .value = 0};
    uint32_t majorStatus =
        gss_display_status(minorStatus, statusValue, statusType, GSS_C_NO_OID, &messageContext, &gssBuffer);

    NetSecurityNative_MoveBuffer(&gssBuffer, outBuffer);
    return majorStatus;
}

uint32_t
NetSecurityNative_DisplayMinorStatus(uint32_t* minorStatus, uint32_t statusValue, struct PAL_GssBuffer* outBuffer)
{
    return NetSecurityNative_DisplayStatus(minorStatus, statusValue, GSS_C_MECH_CODE, outBuffer);
}

uint32_t
NetSecurityNative_DisplayMajorStatus(uint32_t* minorStatus, uint32_t statusValue, struct PAL_GssBuffer* outBuffer)
{
    return NetSecurityNative_DisplayStatus(minorStatus, statusValue, GSS_C_GSS_CODE, outBuffer);
}

uint32_t
NetSecurityNative_ImportUserName(uint32_t* minorStatus, char* inputName, uint32_t inputNameLen, GssName** outputName)
{
    g_assert(minorStatus != 0);
    g_assert(inputName != 0);
    g_assert(outputName != 0);
    g_assert(*outputName == 0);

    GssBuffer inputNameBuffer = {.length = inputNameLen, .value = inputName};
    gss_OID nameType = (gss_OID)GSS_C_NT_USER_NAME;
    return gss_import_name(minorStatus, &inputNameBuffer, nameType, outputName);
}

uint32_t NetSecurityNative_ImportPrincipalName(uint32_t* minorStatus,
                                                          char* inputName,
                                                          uint32_t inputNameLen,
                                                          GssName** outputName)
{
    g_assert(minorStatus != 0);
    g_assert(inputName != 0);
    g_assert(outputName != 0);
    g_assert(*outputName == 0);

    gss_OID nameType;

    if (strchr(inputName, '/') != 0)
    {
        nameType = (gss_OID)GSS_KRB5_NT_PRINCIPAL_NAME;
    }
    else
    {
        nameType = (gss_OID)GSS_C_NT_HOSTBASED_SERVICE;
    }

    GssBuffer inputNameBuffer = {.length = inputNameLen, .value = inputName};
    return gss_import_name(minorStatus, &inputNameBuffer, nameType, outputName);
}

uint32_t NetSecurityNative_InitSecContext(uint32_t* minorStatus,
                                                     GssCredId* claimantCredHandle,
                                                     GssCtxId** contextHandle,
                                                     uint32_t isNtlm,
                                                     GssName* targetName,
                                                     uint32_t reqFlags,
                                                     uint8_t* inputBytes,
                                                     uint32_t inputLength,
                                                     struct PAL_GssBuffer* outBuffer,
                                                     uint32_t* retFlags,
                                                     int32_t* isNtlmUsed)
{
    g_assert(minorStatus != 0);
    g_assert(contextHandle != 0);
    g_assert(isNtlm == 0 || isNtlm == 1);
    g_assert(targetName != 0);
    g_assert(inputBytes != 0 || inputLength == 0);
    g_assert(outBuffer != 0);
    g_assert(retFlags != 0);
    g_assert(isNtlmUsed != 0);
    g_assert(inputBytes != 0 || inputLength == 0);

// Note: claimantCredHandle can be null
// Note: *contextHandle is null only in the first call and non-null in the subsequent calls

#if defined(GSS_SPNEGO_MECHANISM)
    gss_OID desiredMech;
    if (isNtlm)
    {
        desiredMech = GSS_NTLM_MECHANISM;
    }
    else
    {
        desiredMech = GSS_SPNEGO_MECHANISM;
    }

    gss_OID krbMech = GSS_KRB5_MECHANISM;
#else
    gss_OID_desc gss_mech_OID_desc;
    if (isNtlm)
    {
        gss_mech_OID_desc = gss_mech_ntlm_OID_desc;
    }
    else
    {
        gss_mech_OID_desc = gss_mech_spnego_OID_desc;
    }

    gss_OID desiredMech = &gss_mech_OID_desc;
    gss_OID krbMech = (gss_OID)gss_mech_krb5;
#endif

    *isNtlmUsed = 1;
    GssBuffer inputToken = {.length = inputLength, .value = inputBytes};
    GssBuffer gssBuffer = {.length = 0, .value = 0};
    gss_OID_desc* outmech;

    uint32_t majorStatus = gss_init_sec_context(minorStatus,
                                                claimantCredHandle,
                                                contextHandle,
                                                targetName,
                                                desiredMech,
                                                reqFlags,
                                                0,
                                                GSS_C_NO_CHANNEL_BINDINGS,
                                                &inputToken,
                                                &outmech,
                                                &gssBuffer,
                                                retFlags,
                                                0);

    // Outmech can be null when gssntlmssp lib uses NTLM mechanism
    if (outmech != 0 && gss_oid_equal(outmech, krbMech) != 0)
    {
        *isNtlmUsed = 0;
    }

    NetSecurityNative_MoveBuffer(&gssBuffer, outBuffer);
    return majorStatus;
}

uint32_t NetSecurityNative_AcceptSecContext(uint32_t* minorStatus,
                                                       GssCtxId** contextHandle,
                                                       uint8_t* inputBytes,
                                                       uint32_t inputLength,
                                                       struct PAL_GssBuffer* outBuffer)
{
    g_assert(minorStatus != 0);
    g_assert(contextHandle != 0);
    g_assert(inputBytes != 0 || inputLength == 0);
    g_assert(outBuffer != 0);
    // Note: *contextHandle is null only in the first call and non-null in the subsequent calls

    GssBuffer inputToken = {.length = inputLength, .value = inputBytes};
    GssBuffer gssBuffer = {.length = 0, .value = 0};

    uint32_t majorStatus = gss_accept_sec_context(minorStatus,
                                                  contextHandle,
                                                  GSS_C_NO_CREDENTIAL,
                                                  &inputToken,
                                                  GSS_C_NO_CHANNEL_BINDINGS,
                                                  0,
                                                  0,
                                                  &gssBuffer,
                                                  0,
                                                  0,
                                                  0);

    NetSecurityNative_MoveBuffer(&gssBuffer, outBuffer);
    return majorStatus;
}

uint32_t NetSecurityNative_ReleaseCred(uint32_t* minorStatus, GssCredId** credHandle)
{
    g_assert(minorStatus != 0);
    g_assert(credHandle != 0);

    return gss_release_cred(minorStatus, credHandle);
}

void NetSecurityNative_ReleaseGssBuffer(void* buffer, uint64_t length)
{
    g_assert(buffer != 0);

    uint32_t minorStatus;
    GssBuffer gssBuffer = {.length = (size_t)length, .value = buffer};
    gss_release_buffer(&minorStatus, &gssBuffer);
}

uint32_t NetSecurityNative_ReleaseName(uint32_t* minorStatus, GssName** inputName)
{
    g_assert(minorStatus != 0);
    g_assert(inputName != 0);

    return gss_release_name(minorStatus, inputName);
}

uint32_t NetSecurityNative_Wrap(uint32_t* minorStatus,
                                           GssCtxId* contextHandle,
                                           int32_t isEncrypt,
                                           uint8_t* inputBytes,
                                           int32_t offset,
                                           int32_t count,
                                           struct PAL_GssBuffer* outBuffer)
{
    g_assert(minorStatus != 0);
    g_assert(contextHandle != 0);
    g_assert(isEncrypt == 1 || isEncrypt == 0);
    g_assert(inputBytes != 0);
    g_assert(offset >= 0);
    g_assert(count >= 0);
    g_assert(outBuffer != 0);
    // count refers to the length of the input message. That is, number of bytes of inputBytes
    // starting at offset that need to be wrapped.

    int confState;
    GssBuffer inputMessageBuffer = {.length = count, .value = inputBytes + offset};
    GssBuffer gssBuffer;
    uint32_t majorStatus =
        gss_wrap(minorStatus, contextHandle, isEncrypt, GSS_C_QOP_DEFAULT, &inputMessageBuffer, &confState, &gssBuffer);

    NetSecurityNative_MoveBuffer(&gssBuffer, outBuffer);
    return majorStatus;
}

uint32_t NetSecurityNative_Unwrap(uint32_t* minorStatus,
                                             GssCtxId* contextHandle,
                                             uint8_t* inputBytes,
                                             int32_t offset,
                                             int32_t count,
                                             struct PAL_GssBuffer* outBuffer)
{
    g_assert(minorStatus != 0);
    g_assert(contextHandle != 0);
    g_assert(inputBytes != 0);
    g_assert(offset >= 0);
    g_assert(count >= 0);
    g_assert(outBuffer != 0);

    // count refers to the length of the input message. That is, the number of bytes of inputBytes
    // starting at offset that need to be wrapped.
    GssBuffer inputMessageBuffer = {.length = count, .value = inputBytes + offset};
    GssBuffer gssBuffer = {.length = 0, .value = 0};
    uint32_t majorStatus = gss_unwrap(minorStatus, contextHandle, &inputMessageBuffer, &gssBuffer, 0, 0);
    NetSecurityNative_MoveBuffer(&gssBuffer, outBuffer);
    return majorStatus;
}

static uint32_t NetSecurityNative_AcquireCredWithPassword(uint32_t* minorStatus,
                                                          int32_t isNtlm,
                                                          GssName* desiredName,
                                                          char* password,
                                                          uint32_t passwdLen,
                                                          gss_cred_usage_t credUsage,
                                                          GssCredId** outputCredHandle)
{
    g_assert(minorStatus != 0);
    g_assert(isNtlm == 1 || isNtlm == 0);
    g_assert(desiredName != 0);
    g_assert(password != 0);
    g_assert(outputCredHandle != 0);
    g_assert(*outputCredHandle == 0);

#if defined(GSS_SPNEGO_MECHANISM)
    (void)isNtlm; // unused
    // Specifying GSS_SPNEGO_MECHANISM as a desiredMech on OSX fails.
    gss_OID_set desiredMech = GSS_C_NO_OID_SET;
#else
    gss_OID_desc gss_mech_OID_desc;
    if (isNtlm)
    {
        gss_mech_OID_desc = gss_mech_ntlm_OID_desc;
    }
    else
    {
        gss_mech_OID_desc = gss_mech_spnego_OID_desc;
    }

    gss_OID_set_desc gss_mech_OID_set_desc = {.count = 1, .elements = &gss_mech_OID_desc};
    gss_OID_set desiredMech = &gss_mech_OID_set_desc;
#endif

    GssBuffer passwordBuffer = {.length = passwdLen, .value = password};
    uint32_t majorStatus = gss_acquire_cred_with_password(
        minorStatus, desiredName, &passwordBuffer, 0, desiredMech, credUsage, outputCredHandle, 0, 0);

    // call gss_set_cred_option with GSS_KRB5_CRED_NO_CI_FLAGS_X to support Kerberos Sign Only option from *nix client against a windows server
#if defined(GSS_KRB5_CRED_NO_CI_FLAGS_X)
    if (majorStatus == GSS_S_COMPLETE)
    {
        GssBuffer emptyBuffer = GSS_C_EMPTY_BUFFER;
        majorStatus = gss_set_cred_option(minorStatus, outputCredHandle, GSS_KRB5_CRED_NO_CI_FLAGS_X, &emptyBuffer);
    }
#endif

    return majorStatus;
}

uint32_t NetSecurityNative_InitiateCredWithPassword(uint32_t* minorStatus,
                                                               int32_t isNtlm,
                                                               GssName* desiredName,
                                                               char* password,
                                                               uint32_t passwdLen,
                                                               GssCredId** outputCredHandle)
{
    return NetSecurityNative_AcquireCredWithPassword(
        minorStatus, isNtlm, desiredName, password, passwdLen, GSS_C_INITIATE, outputCredHandle);
}
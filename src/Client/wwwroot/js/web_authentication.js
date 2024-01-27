const coerceToArrayBuffer = (x) => {
    const fix = x.replace(/-/g, "+").replace(/_/g, "/");
    return Uint8Array.from(window.atob(fix), c => c.charCodeAt(0));
};

const coerceToBase64Url = (x) => {
    const str = new Uint8Array(x)
        .reduce((acc, x) => acc += String.fromCharCode(x), "");
    return window.btoa(str)
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=*$/g, "");
};

window.ProcessRegistration = async function ProcessRegistration(registrationOptions) {

    const publicKeyCredentialCreationOptions = {
        ...registrationOptions,
        challenge: coerceToArrayBuffer(registrationOptions.challenge),
        user: {
            ...registrationOptions.user,
            id: coerceToArrayBuffer(registrationOptions.user.id),
        },
        excludeCredentials: (registrationOptions.excludeCredentials ?? []).map(x => ({
            ...x,
            id: coerceToArrayBuffer(x.id)
        }))
    };

    const credential = await navigator.credentials.create({ publicKey: publicKeyCredentialCreationOptions });

    console.log(credential);

    const clientExtensionResults = credential.getClientExtensionResults ?
        (credential.getClientExtensionResults() ?? {}) : {};

    const authenticatorData = credential.response.getAuthenticatorData ?
        coerceToBase64Url(credential.response.getAuthenticatorData()) : undefined;

    const responsePublicKey = credential.response.getPublicKey ?
        credential.response.getPublicKey() : undefined;
    const publicKey = responsePublicKey ? coerceToBase64Url(responsePublicKey) : undefined;

    const transports = credential.response.getTransports ?
        credential.response.getTransports() : undefined;

    const publicKeyAlgorithm = credential.response.getPublicKeyAlgorithm ?
        credential.response.getPublicKeyAlgorithm() : undefined;

    const data = {
        id: coerceToBase64Url(credential.rawId),
        rawId: coerceToBase64Url(credential.rawId),
        response: {
            clientDataJson: coerceToBase64Url(credential.response.clientDataJSON),
            authenticatorData,
            transports,
            publicKey,
            publicKeyAlgorithm,
            attestationObject: coerceToBase64Url(credential.response.attestationObject)
        },
        authenticatorAttachment: credential?.authenticatorAttachment,
        clientExtensionResults,
        type: credential.type
    };

    return data;
}

window.ProcessAuthentication = async function ProcessAuthentication(authenticationOptions) {

    const publicKey = {
        ...authenticationOptions,
        challenge: coerceToArrayBuffer(authenticationOptions.challenge),
        allowCredentials: (authenticationOptions.allowCredentials ?? []).map(x => ({ ...x, id: coerceToArrayBuffer(x.id) }))
    };

    const credential = await navigator.credentials.get({ publicKey });

    const clientExtensionResults = credential.getClientExtensionResults ?
        (credential.getClientExtensionResults() ?? {}) : undefined;

    const userHandle = credential.response.userHandle ?
        coerceToBase64Url(credential.response.userHandle) : undefined;

    const attestationObject = credential.response.attestationObject ?
        coerceToBase64Url(credential.response.attestationObject) : undefined;

    const data = {
        id: coerceToBase64Url(credential.rawId),
        rawId: coerceToBase64Url(credential.rawId),
        response: {
            clientDataJSON: coerceToBase64Url(credential.response.clientDataJSON),
            authenticatorData: coerceToBase64Url(credential.response.authenticatorData),
            signature: coerceToBase64Url(credential.response.signature),
            userHandle,
            attestationObject,
        },
        authenticatorAttachment: credential.authenticatorAttachment,
        clientExtensionResults,
        type: credential.type
    }

    return data;
}

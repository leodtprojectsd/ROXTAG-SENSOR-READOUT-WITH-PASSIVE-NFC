var a00187 =
[
    [ "MSG_CMD_READREGISTER_T", "a00187.html#a00252", [
      [ "address", "a00187.html#ac0d31ca829f934cccd89f8054e02773e", null ]
    ] ],
    [ "MSG_CMD_WRITEREGISTER_T", "a00187.html#a00256", [
      [ "address", "a00187.html#ac0d31ca829f934cccd89f8054e02773e", null ],
      [ "data", "a00187.html#a1e43bf7d608e87228b625cca2c04d641", null ]
    ] ],
    [ "MSG_CMD_READMEMORY_T", "a00187.html#a00260", [
      [ "address", "a00187.html#ac0d31ca829f934cccd89f8054e02773e", null ],
      [ "length", "a00187.html#ab2b3adeb2a67e656ff030b56727fd0ac", null ]
    ] ],
    [ "MSG_CMD_WRITEMEMORY_T", "a00187.html#a00264", [
      [ "address", "a00187.html#ac0d31ca829f934cccd89f8054e02773e", null ],
      [ "length", "a00187.html#ab2b3adeb2a67e656ff030b56727fd0ac", null ],
      [ "data", "a00187.html#a6460a21fbea84550b8bf9c7ce257e4ed", null ]
    ] ],
    [ "MSG_RESPONSE_RESULTONLY_T", "a00187.html#a00268", [
      [ "result", "a00187.html#a36692bbc61358ebc0e37a6fc6a395d28", null ]
    ] ],
    [ "MSG_RESPONSE_GETVERSION_T", "a00187.html#a00272", [
      [ "reserved2", "a00187.html#a0fc429b055e74830a4583ec37f5c3846", null ],
      [ "swMajorVersion", "a00187.html#a1091c1725794f79dff1772e544cf4d10", null ],
      [ "swMinorVersion", "a00187.html#ae81df9b4aba1c86ab8dbef8177a25409", null ],
      [ "apiMajorVersion", "a00187.html#aa05ecf2cd4bc24cfd163a5d06adc5b30", null ],
      [ "apiMinorVersion", "a00187.html#ac38d834dbff84b55b6d7348f7a06f5ff", null ],
      [ "deviceId", "a00187.html#a21079ffa80688b89c7dd4ba646a55283", null ]
    ] ],
    [ "MSG_RESPONSE_READREGISTER_T", "a00187.html#a00276", [
      [ "result", "a00187.html#a36692bbc61358ebc0e37a6fc6a395d28", null ],
      [ "data", "a00187.html#a1e43bf7d608e87228b625cca2c04d641", null ]
    ] ],
    [ "MSG_RESPONSE_READMEMORY_T", "a00187.html#a00280", [
      [ "result", "a00187.html#a36692bbc61358ebc0e37a6fc6a395d28", null ],
      [ "length", "a00187.html#ab2b3adeb2a67e656ff030b56727fd0ac", null ],
      [ "data", "a00187.html#a6460a21fbea84550b8bf9c7ce257e4ed", null ]
    ] ],
    [ "MSG_RESPONSE_GETUID_T", "a00187.html#a00284", [
      [ "uid", "a00187.html#a85767879ec473da0d1013ee35b8e7e61", null ]
    ] ],
    [ "MSG_RESPONSE_GETNFCUID_T", "a00187.html#a00288", [
      [ "nfcuid", "a00187.html#aa9aacba25c4393f3e85c22e520de03f6", null ]
    ] ],
    [ "MSG_RESPONSE_CHECKBATTERY_T", "a00187.html#a00292", [
      [ "result", "a00187.html#a36692bbc61358ebc0e37a6fc6a395d28", null ],
      [ "threshold", "a00187.html#ac6bb16831f8103061e123864d8a65b9b", null ]
    ] ],
    [ "MSG_API_MAJOR_VERSION", "a00187.html#ga0248ef44b6dac231165ddc7d5307bf4b", null ],
    [ "MSG_API_MINOR_VERSION", "a00187.html#ga4e54c3655143e65b486041fbb9495dd7", null ],
    [ "MSG_ID_T", "a00187.html#gade7075e3d56378af75d2c2dd4110ddd7", [
      [ "MSG_ID_GETRESPONSE", "a00187.html#ggade7075e3d56378af75d2c2dd4110ddd7ac6247e0367baa5ca8abdf3711dad9f62", null ],
      [ "MSG_ID_GETVERSION", "a00187.html#ggade7075e3d56378af75d2c2dd4110ddd7acf2027afe00dc0d127e562519f67fc5e", null ],
      [ "MSG_ID_RESET", "a00187.html#ggade7075e3d56378af75d2c2dd4110ddd7a3bf58e035a33e2c3267b498fd3316247", null ],
      [ "MSG_ID_READREGISTER", "a00187.html#ggade7075e3d56378af75d2c2dd4110ddd7a8f3dd5bfaa47e8b850625548cb188949", null ],
      [ "MSG_ID_WRITEREGISTER", "a00187.html#ggade7075e3d56378af75d2c2dd4110ddd7a2dce36d3a10962f8c442796565673d21", null ],
      [ "MSG_ID_READMEMORY", "a00187.html#ggade7075e3d56378af75d2c2dd4110ddd7afa343aeadb8e072e4d410ee78a413d4a", null ],
      [ "MSG_ID_WRITEMEMORY", "a00187.html#ggade7075e3d56378af75d2c2dd4110ddd7a4fc3462ad23093891a0e350c2df257ca", null ],
      [ "MSG_ID_PREPAREDEBUG", "a00187.html#ggade7075e3d56378af75d2c2dd4110ddd7a9bd9f498b8f9a5b955f3c7005fa36e17", null ],
      [ "MSG_ID_GETUID", "a00187.html#ggade7075e3d56378af75d2c2dd4110ddd7a5d65ba46817125e29fa7e98bdd23b6e4", null ],
      [ "MSG_ID_GETNFCUID", "a00187.html#ggade7075e3d56378af75d2c2dd4110ddd7a980715d23a96a03e1da157040b961f76", null ],
      [ "MSG_ID_CHECKBATTERY", "a00187.html#ggade7075e3d56378af75d2c2dd4110ddd7a58a0ffc4072e37fb58411bf724354a13", null ],
      [ "MSG_ID_GETCALIBRATIONTIMESTAMP", "a00187.html#ggade7075e3d56378af75d2c2dd4110ddd7ae3359e88c6f64e52a958ae5b299e1eea", null ],
      [ "MSG_ID_GETDIAGDATA", "a00187.html#ggade7075e3d56378af75d2c2dd4110ddd7a5702e7509a5ae498d2144338489b5539", null ],
      [ "MSG_ID_LASTRESERVED", "a00187.html#ggade7075e3d56378af75d2c2dd4110ddd7a9187cb780e8c0547614f8f811e5389e3", null ]
    ] ],
    [ "MSG_ERR_T", "a00187.html#gac3a37ecb0a07b0937ff46b3f4ae8a1ee", [
      [ "MSG_OK", "a00187.html#ggac3a37ecb0a07b0937ff46b3f4ae8a1eea7db209a18c6374183567534787dccc1b", null ],
      [ "MSG_ERR_UNKNOWN_COMMAND", "a00187.html#ggac3a37ecb0a07b0937ff46b3f4ae8a1eea12c639ca7e6f25cee0e4e718d9295488", null ],
      [ "MSG_ERR_NO_RESPONSE", "a00187.html#ggac3a37ecb0a07b0937ff46b3f4ae8a1eead3e97244f77e53026b8e935c3f99faf9", null ],
      [ "MSG_ERR_INVALID_COMMAND_SIZE", "a00187.html#ggac3a37ecb0a07b0937ff46b3f4ae8a1eea4619c227e4354854d1c46b782d3abee3", null ],
      [ "MSG_ERR_INVALID_PARAMETER", "a00187.html#ggac3a37ecb0a07b0937ff46b3f4ae8a1eea049363b30fe169c7ff542e3c0fb7a80f", null ],
      [ "MSG_ERR_INVALID_PRECONDITION", "a00187.html#ggac3a37ecb0a07b0937ff46b3f4ae8a1eeada8577afb9741fca23e048a17a2cb581", null ],
      [ "MSG_ERR_INVALID_NYI", "a00187.html#ggac3a37ecb0a07b0937ff46b3f4ae8a1eea22a4e1b9cb7c2a35f21bc39d61186f2f", null ],
      [ "MSG_ERR_LASTRESERVED", "a00187.html#ggac3a37ecb0a07b0937ff46b3f4ae8a1eea750c656d7ebb7a302c55b8c928520d9b", null ]
    ] ]
];
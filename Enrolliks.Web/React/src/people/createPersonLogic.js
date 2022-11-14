const { createPersonResultType } = require('./personEndpoints');

exports.initialize = validateName => ({
    name: '',
    nameError: validateName(''),
    showNameError: false,
    requestInProgress: false,
    requestStatus: '',
});

exports.setName = (validateName, newName) => ({
    name: newName,
    nameError: validateName(newName),
    showNameError: true,
});

exports.create = (endpoint, state) => {
    if (state.requestInProgress) {
        return [{}, null];
    }

    if (state.nameError.length > 0) {
        return [{}, null];
    }

    const stateUpdateBeforeRequest = {
        requestInProgress: true,
        requestStatus: 'Creating...',
    };

    const executeRequest = async () => {
        const stateUpdateAfterRequest = {
            requestInProgress: false,
        };

        let response;
        try {
            response = await endpoint({
                name: state.name,
            });
        } catch {
            response = null;
        }

        switch (response?.tag) {
            case createPersonResultType.success:
                stateUpdateAfterRequest.requestStatus = '';
                break;
            case createPersonResultType.conflict:
                stateUpdateAfterRequest.requestStatus = 'The person with the name provided already exists.';
                break;
            case createPersonResultType.validationFailure:
                stateUpdateAfterRequest.requestStatus = 'The server returned validation error(s).';
                break;
            case createPersonResultType.badRequest:
                stateUpdateAfterRequest.requestStatus = 'Something went wrong when sending the request.';
                break;
            case createPersonResultType.serverError:
                stateUpdateAfterRequest.requestStatus = 'Something went wrong on the server.';
                break;
            default:
                stateUpdateAfterRequest.requestStatus = 'Something went wrong.';
                break;
        }

        return stateUpdateAfterRequest;
    };

    return [stateUpdateBeforeRequest, executeRequest];
}
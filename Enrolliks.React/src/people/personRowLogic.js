const { deletePersonResultType } = require('./personEndpoints');

exports.initialize = () => ({
    requestInProgress: false,
    requestStatus: '',
});

exports.delete = (endpoint, state, personName) => {
    if (state.requestInProgress === true) {
        return [{}, null];
    }

    const stateUpdateBeforeRequest = {
        requestInProgress: true,
    };

    const stateUpdateAfterRequest = {
        requestInProgress: false,
    };

    const continuation = async () => {
        let response;
        try {
            response = await endpoint(personName);
        } catch {
            response = null;
        }

        switch (response?.tag) {
            case deletePersonResultType.success:
                stateUpdateAfterRequest.requestStatus = '';
                break;

            case deletePersonResultType.notFound:
                stateUpdateAfterRequest.requestStatus = `A person with Name "${personName}" not found.`;
                break;

            case deletePersonResultType.badRequest:
                stateUpdateAfterRequest.requestStatus = 'An error happened while sending a request.';
                break;

            case deletePersonResultType.serverError:
                stateUpdateAfterRequest.requestStatus = 'An error happened on the server.';
                break;

            case deletePersonResultType.unknownError:
                stateUpdateAfterRequest.requestStatus = 'Something went wrong.';
                break;

            default:
                stateUpdateAfterRequest.requestStatus = 'Something very bad and unexpected happened.';
                break;
        }

        return stateUpdateAfterRequest;
    };

    return [stateUpdateBeforeRequest, continuation];
};
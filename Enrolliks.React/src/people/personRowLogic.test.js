const deleteLogic = require('./personRowLogic');
const { deletePersonResultType } = require('./personEndpoints');

describe('Delete person logic', () => {
    const successfulEndpoint = jest.fn();
    successfulEndpoint.mockName('Successful endpoint');
    successfulEndpoint.mockResolvedValue({
        tag: deletePersonResultType.success,
    });

    const notFoundEndpoint = jest.fn();
    notFoundEndpoint.mockName('Not found endpoint');
    notFoundEndpoint.mockResolvedValue({
        tag: deletePersonResultType.notFound,
    });

    const badRequestEndpoint = jest.fn();
    badRequestEndpoint.mockName('Bad request endpoint');
    badRequestEndpoint.mockResolvedValue({
        tag: deletePersonResultType.badRequest,
    });

    const serverErrorEndpoint = jest.fn();
    serverErrorEndpoint.mockName('Server error endpoint.');
    serverErrorEndpoint.mockResolvedValue({
        tag: deletePersonResultType.serverError,
    })

    const unknownErrorEndpoint = jest.fn();
    unknownErrorEndpoint.mockName('Unknown error endpoint.');
    unknownErrorEndpoint.mockResolvedValue({
        tag: deletePersonResultType.unknownError,
    });

    const throwingEndpoint = jest.fn();throwingEndpoint.reset
    throwingEndpoint.mockName('Throwing endpoint');
    throwingEndpoint.mockImplementation(() => Promise.reject(new Error('An endpoint threw an error.')));

    const endpointsAndStatuses = [
        [successfulEndpoint, () => ''],
        [notFoundEndpoint, name => `A person with Name "${name}" not found.`],
        [badRequestEndpoint, () => 'An error happened while sending a request.'],
        [serverErrorEndpoint, () => 'An error happened on the server.'],
        [unknownErrorEndpoint, () => 'Something went wrong.'],
        [throwingEndpoint, () => 'Something very bad and unexpected happened.']
    ];

    const endpoints = endpointsAndStatuses.map(([endpoint]) => endpoint);
    
    beforeEach(() => {
        endpoints.forEach(endpoint => endpoint.mockClear());
    });

    it('Initializes correctly', () => {
        const initialState = deleteLogic.initialize();

        expect(initialState).toEqual({
            requestInProgress: false,
            requestStatus: '',
        });
    });

    it.each(endpoints)('Does not start a request if another one is in progress for %p', endpoint => {
        const initialState = {
            requestInProgress: true,
            requestStatus: '',
        };

        const personName = 'Max Caulfield';

        const [stateUpdateBeforeRequest, continuation] = deleteLogic.delete(endpoint, initialState, personName);

        expect(stateUpdateBeforeRequest).toEqual({});
        expect(continuation).toBe(null);
        expect(endpoint).not.toHaveBeenCalled();
    });

    it.each(endpoints)('Starts a request if there is no other requests for %p', endpoint => {
        const initialState = {
            requestInProgress: false,
            requestStatus: '',
        };

        const personName = 'Max Caulfield';

        const [stateUpdateBeforeRequest, continuation] = deleteLogic.delete(endpoint, initialState, personName);

        expect(stateUpdateBeforeRequest).toEqual({
            requestInProgress: true,
        });
        expect(continuation).not.toBe(null);
    });

    it.each(endpointsAndStatuses)('Completes a request for %p', async (endpoint, expectedStatus) => {
        const initialState = {
            requestInProgress: false,
            requestStatus: '',
        };

        const personName = 'Max Caulfield';

        const [, continuation] = deleteLogic.delete(endpoint, initialState, personName);
        const stateUpdateAfterRequest = await continuation();

        expect(stateUpdateAfterRequest).toEqual({
            requestInProgress: false,
            requestStatus: expectedStatus(personName),
        });

        expect(endpoint).toHaveBeenCalled();
    });
});
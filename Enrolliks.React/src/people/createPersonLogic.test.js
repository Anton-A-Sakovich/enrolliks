const logic = require('./createPersonLogic');
const { createPersonResultType } = require('./personEndpoints');

describe('Create person logic', () => {
    const validateName = name => `[${name}]`;

    const successfulEndpoint = async personToCreate => ({
        tag: createPersonResultType.success,
        createdPerson: personToCreate,
    });

    const validationErrorEndpoint = async () => ({
        tag: createPersonResultType.validationFailure,
        validationErrors: {},
    });

    const badRequestEndpoint = async () => ({
        tag: createPersonResultType.badRequest,
    });

    const conflictEndpoint = async () => ({
        tag: createPersonResultType.conflict,
    });

    const serverErrorEndpoint = async () => ({
        tag: createPersonResultType.serverError,
    });

    const unknownErrorEndpoint = async () => ({
        tag: createPersonResultType.unknownError,
    });

    const throwingEndpoint = async () => {
        throw new Error('The endpoint threw an error.');
    };

    const allEndpointsAndStatuses = [
        [successfulEndpoint, ''],
        [validationErrorEndpoint, 'The server returned validation error(s).'],
        [badRequestEndpoint, 'Something went wrong when sending the request.'],
        [conflictEndpoint, 'The person with the name provided already exists.'],
        [serverErrorEndpoint, 'Something went wrong on the server.'],
        [unknownErrorEndpoint, 'Something went wrong.'],
        [throwingEndpoint, 'Something went wrong.'],
    ];

    const maState = requestInProgress => ({
        name: 'Ma',
        nameError: 'Too short',
        showNameError: true,
        requestInProgress: requestInProgress,
        requestStatus: '',
    });

    const maxState = requestInProgress => ({
        name: 'Max',
        nameError: '',
        showNameError: true,
        requestInProgress: requestInProgress,
        requestStatus: '',
    });

    it('Initializes correctly', () => {
        const initialName = '';

        expect(logic.initialize(validateName)).toEqual({
            name: initialName,
            nameError: validateName(initialName),
            showNameError: false,
            requestInProgress: false,
            requestStatus: ''
        });
    });

    it('Sets the name regardless of anything', () => {
        // This ensures interactivity if the input has not been disabled for some reason.
        // The user cannot initiate parallel requests anyway.
        
        const maxName = 'Max';
        const maxStateUpdate = {
            name: maxName,
            nameError: validateName(maxName),
            showNameError: true,
        };

        expect(logic.setName(validateName, maxName, maState(false))).toEqual(maxStateUpdate);
        expect(logic.setName(validateName, maxName, maState(true))).toEqual(maxStateUpdate);

        const maxiName = 'Maxi';
        const maxiStateUpdate = {
            name: maxiName,
            nameError: validateName(maxiName),
            showNameError: true,
        };

        expect(logic.setName(validateName, maxiName, maxState(false))).toEqual(maxiStateUpdate);
        expect(logic.setName(validateName, maxiName, maxState(true))).toEqual(maxiStateUpdate);
    });

    it('Starts showing an error after the first name update', () => {
        const state = {
            name: '',
            nameError: 'Too short',
            showNameError: false,
            requestInProgress: false,
            requestStatus: '',
        };

        const newName = 'M';

        expect(logic.setName(validateName, newName, state)).toEqual({
            name: newName,
            nameError: validateName(newName),
            showNameError: true,
        });
    });

    it('Does not send a request if the name is invalid', () => {
        const endpoint = () => {
            throw new Error('Endpoint must not be called.');
        };

        expect(logic.create(endpoint, maState(false))).toEqual([{}, null]);
    });

    it('Does not send a request if another one is already in progress', () => {
        const endpoint = () => {
            throw new Error('Endpoint must not be called.');
        };

        expect(logic.create(endpoint, maxState(true))).toEqual([{}, null]);
    });

    it.each(allEndpointsAndStatuses.map(([endpoint]) => endpoint))('Sets request in progress to true for endpoint %p', (endpoint) => {
        const [stateUpdateBeforeRequest,] = logic.create(endpoint, maxState(false));
        expect(stateUpdateBeforeRequest.requestInProgress).toBe(true);
    });

    it.each(allEndpointsAndStatuses.map(([endpoint]) => endpoint))('Sets request in progress to false for endpoint %p', async (endpoint) => {
        const [, executeRequest] = logic.create(endpoint, maxState(false));
        const stateUpdateAfterRequest = await executeRequest();
        expect(stateUpdateAfterRequest.requestInProgress).toBe(false);
    });

    it.each(allEndpointsAndStatuses)('Sets the correct request status for endpoint %p', async (endpoint, expectedStatus) => {
        const [stateUpdateBeforeRequest, executeRequest] = logic.create(endpoint, maxState(false));
        
        expect(stateUpdateBeforeRequest.requestStatus).toBe('Creating...');
        const stateUpdateAfterRequest = await executeRequest();
        expect(stateUpdateAfterRequest.requestStatus).toBe(expectedStatus);
    });
});
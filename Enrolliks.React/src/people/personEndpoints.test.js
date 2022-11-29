const {
    createPerson,
    createPersonResultType,

    deletePerson,
    deletePersonResultType,
} = require('./personEndpoints');

describe('Create person endpoint', () => {
    const createUrl = '/api/people/create';

    const maxCaulfield = {
        name: 'Max Caulfield',
    };

    const validationErrors = {
        name: {
            tag: 'Too Long',
            value: {
                maxCharactersAllowed: 10,
            },
        },
    };

    const conversionData = [
        {
            postReturnedValue: {
                status: 201,
                data: Object.assign({}, maxCaulfield),
            },
            returnedValue: {
                tag: createPersonResultType.success,
                createdPerson: Object.assign({}, maxCaulfield),
            },
        },
        {
            postReturnedValue: {
                status: 400,
                data: validationErrors,
            },
            returnedValue: {
                tag: createPersonResultType.validationFailure,
                validationErrors: validationErrors,
            },
        },
        {
            postReturnedValue: {
                status: 409,
                data: null,
            },
            returnedValue: {
                tag: createPersonResultType.conflict,
            },
        },
        {
            postReturnedValue: {
                status: 404,
                data: "string"
            },
            returnedValue: {
                tag: createPersonResultType.badRequest,
            },
        },
        {
            postReturnedValue: {
                status: 500,
                data: null,
            },
            returnedValue: {
                tag: createPersonResultType.serverError,
            },
        },
        {
            postReturnedValue: {
                status: 403,
                data: null,
            },
            returnedValue: {
                tag: createPersonResultType.unknownError,
            },
        },
        {
            postReturnedValue: {
                status: 503,
                data: null,
            },
            returnedValue: {
                tag: createPersonResultType.unknownError,
            },
        },
    ];

    it.each(conversionData)('Converts HTTP $postReturnedValue.status data to result', async ({postReturnedValue, returnedValue}) => {
        const post = jest.fn();
        post.mockResolvedValue(postReturnedValue);

        expect.assertions(2);
        await expect(createPerson(post, maxCaulfield)).resolves.toEqual(returnedValue);
        expect(post).toHaveBeenCalledWith(createUrl, maxCaulfield);
    });

    it('Passes through errors', async () => {
        const error = new Error();
        const post = jest.fn();
        post.mockImplementation(async () => { throw error; });

        expect.assertions(2);
        await expect(() => createPerson(post, maxCaulfield)).rejects.toThrow(error);
        expect(post).toHaveBeenCalledWith(createUrl, maxCaulfield);
    });
});

describe('Delete person endpoint', () => {
    const deleteUrl = name => `/api/people/${encodeURIComponent(name)}`;
    const maxCaulfield = 'Max Caulfield';

    const conversionData = [
        {
            delReturnedValue: {
                status: 204,
                data: null,
            },
            returnedValue: {
                tag: deletePersonResultType.success,
            }
        },
        {
            delReturnedValue: {
                status: 404,
                data: null,
            },
            returnedValue: {
                tag: deletePersonResultType.notFound,
            }
        },
        {
            delReturnedValue: {
                status: 500,
                data: null,
            },
            returnedValue: {
                tag: deletePersonResultType.serverError,
            }
        },
        {
            delReturnedValue: {
                status: 403,
                data: null,
            },
            returnedValue: {
                tag: deletePersonResultType.unknownError,
            },
        },
        {
            delReturnedValue: {
                status: 503,
                data: null,
            },
            returnedValue: {
                tag: deletePersonResultType.unknownError,
            },
        },
    ];

    it.each(conversionData)('Converts HTTP $delReturnedValue.status data to result', async ({delReturnedValue, returnedValue}) => {
        const del = jest.fn();
        del.mockResolvedValue(delReturnedValue);

        expect.assertions(2);
        await expect(deletePerson(del, maxCaulfield)).resolves.toEqual(returnedValue);
        expect(del).toHaveBeenCalledWith(deleteUrl(maxCaulfield));
    });
});
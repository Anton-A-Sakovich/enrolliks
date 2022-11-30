const {
    createPerson,
    createPersonResultType,

    deletePerson,
    deletePersonResultType,

    getPeople,
    getPeopleResultType,
} = require('./personEndpoints');

describe('Create person endpoint', () => {
    const createPersonUrl = '/api/people/create';

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
        ...(
            [null, undefined, "", "string", [], [maxCaulfield]].map(data => ({
                postReturnedValue: {
                    status: 201,
                    data: data,
                },
                returnedValue: {
                    tag: createPersonResultType.unknownError,
                }
            }))
        ),
        ...(
            [200, 202, 203, 204].map(status => ({
                postReturnedValue: {
                    status: status,
                },
                returnedValue: {
                    tag: createPersonResultType.unknownError,
                },
            }))
        ),
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
        ...(
            [null, undefined, "", "string", [], [validationErrors]].map(data => ({
                postReturnedValue: {
                    status: 400,
                    data: data,
                },
                returnedValue: {
                    tag: createPersonResultType.badRequest,
                },
            }))
        ),
        {
            postReturnedValue: {
                status: 409,
            },
            returnedValue: {
                tag: createPersonResultType.conflict,
            },
        },
        ...(
            [401, 402, 403, 404, 405].map(status => ({
                postReturnedValue: {
                    status: status,
                },
                returnedValue: {
                    tag: createPersonResultType.badRequest,
                },
            }))
        ),
        ...(
            [500, 501, 502, 503, 504].map(status => ({
                postReturnedValue: {
                    status: status,
                },
                returnedValue: {
                    tag: createPersonResultType.serverError,
                },
            }))
        ),
    ];

    it.each(conversionData)('Converts HTTP $postReturnedValue.status data to result', async ({postReturnedValue, returnedValue}) => {
        const post = jest.fn();
        post.mockResolvedValue(postReturnedValue);

        expect.assertions(2);
        await expect(createPerson(post, maxCaulfield)).resolves.toEqual(returnedValue);
        expect(post).toHaveBeenCalledWith(createPersonUrl, maxCaulfield);
    });

    it('Passes through errors', async () => {
        const error = new Error();
        const post = jest.fn();
        post.mockRejectedValue(error);

        expect.assertions(2);
        await expect(() => createPerson(post, maxCaulfield)).rejects.toThrow(error);
        expect(post).toHaveBeenCalledWith(createPersonUrl, maxCaulfield);
    });
});

describe('Delete person endpoint', () => {
    const deletePersonUrl = name => `/api/people/${encodeURIComponent(name)}`;
    const maxCaulfield = 'Max Caulfield';

    const conversionData = [
        {
            delReturnedValue: {
                status: 204,
            },
            returnedValue: {
                tag: deletePersonResultType.success,
            }
        },
        ...(
            [200, 201, 202, 203, 205].map(status => ({
                delReturnedValue: {
                    status: status,
                },
                returnedValue: {
                    tag: deletePersonResultType.unknownError,
                },
            }))
        ),
        {
            delReturnedValue: {
                status: 404,
                data: null,
            },
            returnedValue: {
                tag: deletePersonResultType.notFound,
            }
        },
        ...(
            [0, false, "", [], {}].map(data => ({
                delReturnedValue: {
                    status: 404,
                    data: data,
                },
                returnedValue: {
                    tag: deletePersonResultType.badRequest,
                },
            }))
        ),
        ...(
            [400, 401, 402, 403, 405].map(status => ({
                delReturnedValue: {
                    status: status,
                },
                returnedValue: {
                    tag: deletePersonResultType.badRequest,
                },
            }))
        ),
        ...(
            [500, 501, 502, 503, 505].map(status => ({
                delReturnedValue: {
                    status: status,
                },
                returnedValue: {
                    tag: deletePersonResultType.serverError,
                },
            }))
        ),
    ];

    it.each(conversionData)('Converts HTTP $delReturnedValue.status data to result', async ({delReturnedValue, returnedValue}) => {
        const del = jest.fn();
        del.mockResolvedValue(delReturnedValue);

        expect.assertions(2);
        await expect(deletePerson(del, maxCaulfield)).resolves.toEqual(returnedValue);
        expect(del).toHaveBeenCalledWith(deletePersonUrl(maxCaulfield));
    });

    it('Passes through errors', async () => {
        const del = jest.fn();
        const error = new Error();
        del.mockRejectedValue(error);

        expect.assertions(2);
        await expect(deletePerson(del, maxCaulfield)).rejects.toThrow(error);
        expect(del).toHaveBeenCalledWith(deletePersonUrl(maxCaulfield));
    });
});

describe('Get all people endpoint', () => {
    const getPeopleUrl = '/api/people';

    const people = [
        {
            name: 'Max Caulfield',
        },
        {
            name: 'Chloe Price',
        },
    ];

    const conversionData = [
        {
            getReturnedValue: {
                status: 200,
                data:people,
            },
            returnedValue: {
                tag: getPeopleResultType.success,
                people: people,
            },
        },
        ...(
            [null, undefined, "", "string", ["", "string"]].map(data => ({
                getReturnedValue: {
                    status: 200,
                    data: data,
                },
                returnedValue: {
                    tag: getPeopleResultType.unknownError,
                },
            }))
        ),
        ...(
            [201, 202, 203, 204].map(status => ({
                getReturnedValue: {
                    status: status,
                },
                returnedValue: {
                    tag: getPeopleResultType.unknownError,
                },
            }))
        ),
        ...(
            [400, 401, 402, 403].map(status => ({
                getReturnedValue: {
                    status: status,
                },
                returnedValue: {
                    tag: getPeopleResultType.badRequest,
                },
            }))
        ),
        ...(
            [500, 501, 502, 503].map(status => ({
                getReturnedValue: {
                    status: status,
                },
                returnedValue: {
                    tag: getPeopleResultType.serverError
                },
            }))
        ),
    ];

    it.each(conversionData)('Converts HTTP $getReturnedValue.status data to result', async ({getReturnedValue, returnedValue}) => {
        const get = jest.fn();
        get.mockResolvedValue(getReturnedValue);

        expect.assertions(2);
        await expect(getPeople(get)).resolves.toEqual(returnedValue);
        expect(get).toHaveBeenCalledWith(getPeopleUrl);
    });

    it('Passes through errors', async () => {
        const get = jest.fn();
        const error = new Error();
        get.mockRejectedValue(error);

        await expect(getPeople(get)).rejects.toThrow(error);
        expect(get).toHaveBeenCalledWith(getPeopleUrl);
    });
});
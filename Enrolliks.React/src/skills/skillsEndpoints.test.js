const {
    createSkill,
    createSkillResultType
} = require('./skillsEndpoints');

describe('Create skill', () => {
    const createSkillUrl = '/api/skills/create';

    const skillToCreate = {
        id: 'dot-net',
        name: '.NET',
    };

    const createdSkill = Object.assign({}, skillToCreate);

    const validationErrorsObjects = [
        {
            name: {
                tag: 'Too Long',
                value: {
                    maxCharactersAllowed: 128,
                },
            }
        },
    ];

    const conversionData = [
        {
            postReturns: {
                status: 201,
                data: createdSkill,
            },
            endpointReturns: {
                tag: createSkillResultType.success,
                createdSkill: createdSkill,
            },
        },
        ...(
            [undefined, null, '', 'string', {}, [], ['string'] [{}], [createdSkill]].map(data => ({
                postReturns: {
                    status: 201,
                    data: data,
                },
                endpointReturns: {
                    tag: createSkillResultType.unknownError,
                },
            }))
        ),
        ...(
            [200, 202, 203, 204, 205].map(status => ({
                postReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: createSkillResultType.unknownError,
                },
            }))
        ),
        ...(
            validationErrorsObjects.map(validationErrors => ({
                postReturns: {
                    status: 400,
                    data: validationErrors,
                },
                endpointReturns: {
                    tag: createSkillResultType.validationFailure,
                    validationErrors: validationErrors,
                },
            }))
        ),
        ...(
            [undefined, null, '', 'string', {}, [], ['string']].map(data => ({
                postReturns: {
                    status: 400,
                    data: data,
                },
                endpointReturns: {
                    tag: createSkillResultType.badRequest,
                },
            }))
        ),
        {
            postReturns: {
                status: 409,
            },
            endpointReturns: {
                tag: createSkillResultType.conflict,
            },
        },
        ...(
            [401, 402, 403, 404, 405].map(status => ({
                postReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: createSkillResultType.badRequest,
                },
            }))
        ),
        ...(
            [501, 502, 503, 504, 505].map(status => ({
                postReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: createSkillResultType.serverError,
                },
            }))
        ),
    ];

    it.each(conversionData)('converts HTTP $postReturns.status response', async ({postReturns, endpointReturns}) => {
        const post = jest.fn();
        post.mockResolvedValue(postReturns);

        expect.assertions(2);
        await expect(createSkill(post, skillToCreate)).resolves.toEqual(endpointReturns);
        expect(post.mock.calls).toEqual([[createSkillUrl, skillToCreate]]);
    });
});
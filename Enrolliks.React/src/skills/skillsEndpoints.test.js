const {
    createSkill,
    createSkillResultType,

    deleteSkill,
    deleteSkillResultType,

    getSkills,
    getSkillsResultType,
} = require('./skillsEndpoints');

const generateStatuses = (zeroStatus, except) => Array.from(
    function* () {
        for (let status = zeroStatus; status < zeroStatus + 11; status++) {
            if (!except || except.length === 0 || except.indexOf(status) === -1) {
                yield status;
            }
        }
    }()
);

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
            generateStatuses(200, [201]).map(status => ({
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
            generateStatuses(400, [400, 409]).map(status => ({
                postReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: createSkillResultType.badRequest,
                },
            }))
        ),
        ...(
            generateStatuses(500).map(status => ({
                postReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: createSkillResultType.serverError,
                },
            }))
        ),
    ];

    it.each(conversionData)('Converts HTTP $postReturns.status response', async ({postReturns, endpointReturns}) => {
        const post = jest.fn();
        post.mockResolvedValue(postReturns);

        expect.assertions(2);
        await expect(createSkill(post, skillToCreate)).resolves.toEqual(endpointReturns);
        expect(post.mock.calls).toEqual([[createSkillUrl, skillToCreate]]);
    });

    it('Passes through errors', async () => {
        const post = jest.fn();
        const error = new Error();
        post.mockRejectedValue(error);

        expect.assertions(2);
        await expect(createSkill(post, skillToCreate)).rejects.toBe(error);
        expect(post.mock.calls).toEqual([[createSkillUrl, skillToCreate]]);
    });
});

describe('Delete skill', () => {
    const deleteSkillUrl = id => `/api/skills/${encodeURIComponent(id)}`;
    const skillId = 'dot-net';

    const conversionData = [
        {
            delReturns: {
                status: 204,
            },
            endpointReturns: {
                tag: deleteSkillResultType.success,
            },
        },
        ...(
            generateStatuses(200, [204]).map(status => ({
                delReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: deleteSkillResultType.unknownError,
                },
            }))
        ),
        {
            delReturns: {
                status: 404,
            },
            endpointReturns: {
                tag: deleteSkillResultType.notFound,
            },
        },
        ...(
            generateStatuses(400, [404]).map(status => ({
                delReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: deleteSkillResultType.badRequest,
                },
            }))
        ),
        ...(
            generateStatuses(500).map(status => ({
                delReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: deleteSkillResultType.serverError,
                },
            }))
        ),
    ];

    it.each(conversionData)('Converts HTTP $delReturns.status response', async ({delReturns, endpointReturns}) => {
        const del = jest.fn();
        del.mockResolvedValue(delReturns);

        expect.assertions(2);
        await expect(deleteSkill(del, skillId)).resolves.toEqual(endpointReturns);
        expect(del.mock.calls).toEqual([[deleteSkillUrl(skillId)]]);
    });

    it('Passes through errors', async () => {
        const del = jest.fn();
        const error = new Error();
        del.mockRejectedValue(error);

        expect.assertions(2);
        await expect(deleteSkill(del, skillId)).rejects.toBe(error);
        expect(del.mock.calls).toEqual([[deleteSkillUrl(skillId)]]);
    });
});

describe('Get skills', () => {
    const getSkillsUrl = '/api/skills';

    const skills = [
        {
            id: 'dot-net',
            name: '.NET',
        },
        {
            id: 'react',
            name: 'React',
        }
    ];

    const conversionData = [
        {
            title: '200; expected body.',
            getReturns: {
                status: 200,
                data: skills,
            },
            endpointReturns: {
                tag: getSkillsResultType.success,
                skills: skills,
            },
        },
        ...(
            [undefined, null, '', 'string', {}].map(data => ({
                title: '200; unexpected body.',
                getReturns: {
                    status: 200,
                    data: data,
                },
                endpointReturns: {
                    tag: getSkillsResultType.unknownError,
                },
            }))
        ),
        ...(
            generateStatuses(200, [200]).map(status => ({
                title: '2XX (unexpected).',
                getReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: getSkillsResultType.unknownError,
                },
            }))
        ),
        ...(
            generateStatuses(400).map(status => ({
                title: '4XX (unexpected)',
                getReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: getSkillsResultType.badRequest,
                },
            }))
        ),
        ...(
            generateStatuses(500).map(status => ({
                title: '5XX (unexpected).',
                getReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: getSkillsResultType.serverError,
                },
            }))
        ),
    ];

    it.each(conversionData)('Converts HTTP status code $title', async ({getReturns, endpointReturns}) => {
        const get = jest.fn();
        get.mockResolvedValue(getReturns);

        expect.assertions(2);
        await expect(getSkills(get, getSkillsUrl)).resolves.toEqual(endpointReturns);
        expect(get.mock.calls).toEqual([[getSkillsUrl]]);
    });

    it('Passes through errors', async () => {
        const get = jest.fn();
        const error = new Error();
        get.mockRejectedValue(error);

        expect.assertions(2);
        await expect(getSkills(get, getSkillsUrl)).rejects.toBe(error);
        expect(get.mock.calls).toEqual([[getSkillsUrl]]);
    });
});
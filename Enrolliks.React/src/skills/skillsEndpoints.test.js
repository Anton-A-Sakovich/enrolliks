const {
    createSkill,
    createSkillResultType,

    deleteSkill,
    deleteSkillResultType,

    getSkills,
    getSkillsResultType,

    getSkill,
    getSkillResultType,

    updateSkill,
    updateSkillResultType,
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
            title: '201; expected body.',
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
                title: '201; unexpected body.',
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
                title: '2XX (unexpected).',
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
                title: '400; expected body.',
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
            [undefined, null, '', 'string', {}, [], ['string'], validationErrorsObjects].map(data => ({
                title: '400; unexpected body.',
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
            title: '409; expected body.',
            postReturns: {
                status: 409,
                data: null,
            },
            endpointReturns: {
                tag: createSkillResultType.conflict,
            },
        },
        ...(
            [undefined, '', 'string', {}, []].map(data => ({
                title: '409; unexpected body.',
                postReturns: {
                    status: 409,
                    data: data,
                },
                endpointReturns: {
                    tag: createSkillResultType.badRequest,
                },
            }))
        ),
        ...(
            generateStatuses(400, [400, 409]).map(status => ({
                title: '4XX (unexpected).',
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
                title: '5XX (unexpected).',
                postReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: createSkillResultType.serverError,
                },
            }))
        ),
    ];

    it.each(conversionData)('Converts HTTP status code $title', async ({postReturns, endpointReturns}) => {
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
    const deleteSkillUrl = skillId => `/api/skills/${encodeURIComponent(skillId)}`;
    const skillId = 'dot-net';

    const conversionData = [
        {
            title: '204; expected body.',
            delReturns: {
                status: 204,
            },
            endpointReturns: {
                tag: deleteSkillResultType.success,
            },
        },
        ...(
            generateStatuses(200, [204]).map(status => ({
                title: '204; unexpected body.',
                delReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: deleteSkillResultType.unknownError,
                },
            }))
        ),
        {
            title: '404; expected body.',
            delReturns: {
                status: 404,
                data: {},
            },
            endpointReturns: {
                tag: deleteSkillResultType.notFound,
            },
        },
        ...(
            [null, undefined, '', 'string', []].map(data => ({
                title: '404; unexpected body.',
                delReturns: {
                    status: 404,
                    data: data,
                },
                endpointReturns: {
                    tag: deleteSkillResultType.badRequest,
                },
            }))
        ),
        ...(
            generateStatuses(400, [404]).map(status => ({
                title: '4XX (unexpected).',
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
                title: '5XX (unexpected).',
                delReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: deleteSkillResultType.serverError,
                },
            }))
        ),
    ];

    it.each(conversionData)('Converts HTTP status code $title', async ({delReturns, endpointReturns}) => {
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
                title: '4XX (unexpected).',
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
        await expect(getSkills(get)).resolves.toEqual(endpointReturns);
        expect(get.mock.calls).toEqual([[getSkillsUrl]]);
    });

    it('Passes through errors', async () => {
        const get = jest.fn();
        const error = new Error();
        get.mockRejectedValue(error);

        expect.assertions(2);
        await expect(getSkills(get)).rejects.toBe(error);
        expect(get.mock.calls).toEqual([[getSkillsUrl]]);
    });
});

describe('Get skill', () => {
    const getSkillUrl = skillId => `/api/skills/${encodeURIComponent(skillId)}`;

    const skillId = 'dot-net';

    const skill = {
        id: 'dot-net',
        name: '.NET',
    };

    const conversionData = [
        {
            title: '200; expected body.',
            getReturns: {
                status: 200,
                data: skill,
            },
            endpointReturns: {
                tag: getSkillResultType.success,
                skill: skill,
            },
        },
        ...(
            [undefined, null, '', 'string', {}, [], [skill]].map(data => ({
                title: '200; unexpected body.',
                getReturns: {
                    status: 200,
                    data: data,
                },
                endpointReturns: {
                    tag: getSkillResultType.unknownError,
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
                    tag: getSkillResultType.unknownError,
                },
            }))
        ),
        {
            title: '404; expected body.',
            getReturns: {
                status: 404,
                data: {},
            },
            endpointReturns: {
                tag: getSkillResultType.notFound,
            }
        },
        ...(
            [undefined, null, '', 'string', []].map(data => ({
                title: '404; unexpected body.',
                getReturns: {
                    status: 404,
                    data: data,
                },
                endpointReturns: {
                    tag: getSkillResultType.badRequest,
                }
            }))
        ),
        ...(
            generateStatuses(400).map(status => ({
                title: '4XX (unexpected).',
                getReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: getSkillResultType.badRequest,
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
                    tag: getSkillResultType.serverError,
                },
            }))
        ),
    ];

    it.each(conversionData)('Converts HTTP status code $title', async ({getReturns, endpointReturns}) => {
        const get = jest.fn();
        get.mockResolvedValue(getReturns);

        expect.assertions(2);
        await expect(getSkill(get, skillId)).resolves.toEqual(endpointReturns);
        expect(get.mock.calls).toEqual([[getSkillUrl(skillId)]]);
    });

    it('Passes through errors', async () => {
        const get = jest.fn();
        const error = new Error();
        get.mockRejectedValue(error);

        expect.assertions(2);
        await expect(getSkill(get, skillId)).rejects.toBe(error);
        expect(get.mock.calls).toEqual([[getSkillUrl(skillId)]]);
    });
});

describe('Update skill', () => {
    const updateSkillUrl = skillId => `/api/skills/${encodeURIComponent(skillId)}`;

    const skillToUpdate = {
        id: 'react',
        name: 'ReactJS'
    };

    const updatedSkill = {
        id: 'react',
        name: 'React'
    };

    const validationErrorsObjects = [
        {
            name: {
                tag: 'Too long',
                value: {
                    maxCharactersAllowed: 128,
                },
            },
        },
    ];

    const conversionData = [
        {
            title: '200; expected body.',
            putReturns: {
                status: 200,
                data: updatedSkill,
            },
            endpointReturns: {
                tag: updateSkillResultType.success,
                updatedSkill: updatedSkill,
            },
        },
        ...(
            [undefined, null, '', 'string', {}, [], [updatedSkill]].map(data => ({
                title: '200; unexpected body.',
                putReturns: {
                    status: 200,
                    data: data,
                },
                endpointReturns: {
                    tag: updateSkillResultType.unknownError,
                },
            }))
        ),
        ...(
            generateStatuses(200, [200]).map(status => ({
                title: '2XX (unexpected).',
                putReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: updateSkillResultType.unknownError,
                },
            }))
        ),
        ...(
            validationErrorsObjects.map(validationErrors => ({
                title: '400; expected body.',
                putReturns: {
                    status: 400,
                    data: validationErrors,
                },
                endpointReturns: {
                    tag: updateSkillResultType.validationFailure,
                    validationErrors: validationErrors,
                },
            }))
        ),
        ...(
            [undefined, null, '', 'string', {}, [], validationErrorsObjects].map(data => ({
                title: '400; unexpected body.',
                putReturns: {
                    status: 400,
                    data: data,
                },
                endpointReturns: {
                    tag: updateSkillResultType.badRequest,
                },
            }))
        ),
        {
            title: '404; expected body.',
            putReturns: {
                status: 404,
                data: {},
            },
            endpointReturns: {
                tag: updateSkillResultType.notFound,
            },
        },
        ...(
            [undefined, null, '', 'string', []].map(data => ({
                title: '404; unexpected body.',
                putReturns: {
                    status: 404,
                    data: data,
                },
                endpointReturns: {
                    tag: updateSkillResultType.badRequest,
                },
            }))
        ),
        {
            title: '409; expected body.',
            putReturns: {
                status: 409,
                data: null,
            },
            endpointReturns: {
                tag: updateSkillResultType.conflict,
            },
        },
        ...(
            [undefined, '', 'string', {}, []].map(data => ({
                title: '409; unexpected body.',
                putReturns: {
                    status: 409,
                    data: data,
                },
                endpointReturns: {
                    tag: updateSkillResultType.badRequest,
                },
            }))
        ),
        ...(
            generateStatuses(400, [400, 404, 409]).map(status => ({
                title: '4XX (unexpected).',
                putReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: updateSkillResultType.badRequest,
                },
            }))
        ),
        ...(
            generateStatuses(500).map(status => ({
                title: '5XX (unexpected).',
                putReturns: {
                    status: status,
                },
                endpointReturns: {
                    tag: updateSkillResultType.serverError,
                },
            }))
        ),
    ];

    it.each(conversionData)('Converts HTTP status code $title', async ({putReturns, endpointReturns}) => {
        const put = jest.fn();
        put.mockResolvedValue(putReturns);

        expect.assertions(2);
        await expect(updateSkill(put, skillToUpdate)).resolves.toEqual(endpointReturns);
        expect(put.mock.calls).toEqual([[updateSkillUrl(skillToUpdate.id), skillToUpdate]]);
    });

    it('Passes through errors', async () => {
        const put = jest.fn();
        const error = new Error();
        put.mockRejectedValue(error);

        expect.assertions(2);
        await expect(updateSkill(put, skillToUpdate)).rejects.toBe(error);
        expect(put.mock.calls).toEqual([[updateSkillUrl(skillToUpdate.id), skillToUpdate]]);
    });
});
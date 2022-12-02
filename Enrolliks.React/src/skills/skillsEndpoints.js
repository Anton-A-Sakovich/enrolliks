const { isArray, and, repeated } = require('../predicates');
const { isSkill, isSkillValidationErrors } = require('./skillsUtils');

function defaultConvert(status, resultType) {
    if (Math.trunc(status / 100) === 4) {
        return {
            tag: resultType.badRequest,
        };
    }

    if (Math.trunc(status / 100) === 5) {
        return {
            tag: resultType.serverError,
        };
    }

    return {
        tag: resultType.unknownError,
    };
}

const createSkillResultType = module.exports.createSkillResultType = {
    success: 'createSkillResultType.success',
    validationFailure: 'createSkillResultType.validationFailure',
    conflict: 'createSkillResultType.conflict',
    badRequest: 'createSkillResultType.badRequest',
    serverError: 'createSkillResultType.serverError',
    unknownError: 'createSkillResultType.unknownError',
};

module.exports.createSkill = async function createSkill(post, skillToCreate) {
    const response = await post('/api/skills/create', skillToCreate);

    if (response.status === 201 && isSkill(response.data)) {
        return {
            tag: createSkillResultType.success,
            createdSkill: response.data,
        };
    }

    if (response.status === 400 && isSkillValidationErrors(response.data)) {
        return {
            tag: createSkillResultType.validationFailure,
            validationErrors: response.data,
        };
    }

    if (response.status === 409) {
        return {
            tag: createSkillResultType.conflict,
        };
    }

    return defaultConvert(response.status, createSkillResultType);
};

const deleteSkillResultType = module.exports.deleteSkillResultType = {
    success: 'deleteSkillResultType.success',
    notFound: 'deleteSkillResultType.notFound',
    badRequest: 'deleteSkillResultType.badRequest',
    serverError: 'deleteSkillResultType.serverError',
    unknownError: 'deleteSkillResultType.unknownError',
};

module.exports.deleteSkill = async function deleteSkill(del, skillId) {
    const response = await del(`/api/skills/${encodeURIComponent(skillId)}`);

    if (response.status === 204) {
        return {
            tag: deleteSkillResultType.success,
        };
    }

    if (response.status === 404) {
        return {
            tag: deleteSkillResultType.notFound,
        };
    }

    return defaultConvert(response.status, deleteSkillResultType);
};

const getSkillsResultType = module.exports.getSkillsResultType = {
    success: 'getSkillsResultType.success',
    badRequest: 'getSkillsResultType.badRequest',
    serverError: 'getSkillsResultType.serverError',
    unknownError: 'getSkillsResultType.unknownError',
};

module.exports.getSkills = async function getSkills(get) {
    const response = await get('/api/skills');

    if (response.status === 200 && and(isArray, repeated(isSkill, 0))(response.data)) {
        return {
            tag: getSkillsResultType.success,
            skills: response.data,
        };
    }

    return defaultConvert(response.status, getSkillsResultType);
};
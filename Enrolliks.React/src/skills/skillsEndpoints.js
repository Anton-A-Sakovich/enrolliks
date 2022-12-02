const { isArray, and, repeated, isObject, isConstant } = require('../predicates');
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

    if (response.status === 409 && isConstant(null)(response.data)) {
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

    if (response.status === 404 && isObject(response.data)) {
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

const getSkillResultType = module.exports.getSkillResultType = {
    success: 'getSkillResultType.success',
    notFound: 'getSkillResultType.notFound',
    badRequest: 'getSkillResultType.badRequest',
    serverError: 'getSkillResultType.serverError',
    unknownError: 'getSkillResultType.unknownError',
};

module.exports.getSkill = async function getSkill(get, skillId) {
    const response = await get(`/api/skills/${encodeURIComponent(skillId)}`);

    if (response.status === 200 && isSkill(response.data)) {
        return {
            tag: getSkillResultType.success,
            skill: response.data,
        };
    }

    if (response.status === 404 && isObject(response.data)) {
        return {
            tag: getSkillResultType.notFound,
        };
    }

    return defaultConvert(response.status, getSkillResultType);
};

const updateSkillResultType = module.exports.updateSkillResultType = {
    success: 'updateSkillResultType.success',
    validationFailure: 'updateSkillResultType.validationFailure',
    conflict: 'updateSkillResultType.conflict',
    notFound: 'updateSkillResultType.notFound',
    badRequest: 'updateSkillResultType.badRequest',
    serverError: 'updateSkillResultType.serverError',
    unknownError: 'updateSkillResultType.unknownError',
};

module.exports.updateSkill = async function updateSkill(put, skillToUpdate) {
    const response = await put(`/api/skills/${encodeURIComponent(skillToUpdate.id)}`, skillToUpdate);

    if (response.status === 200 && isSkill(response.data)) {
        return {
            tag: updateSkillResultType.success,
            updatedSkill: response.data,
        };
    }

    if (response.status === 400 && isSkillValidationErrors(response.data)) {
        return {
            tag: updateSkillResultType.validationFailure,
            validationErrors: response.data,
        };
    }

    if (response.status === 404 && isObject(response.data)) {
        return {
            tag: updateSkillResultType.notFound,
        };
    }

    if (response.status === 409 && isConstant(null)(response.data)) {
        return {
            tag: updateSkillResultType.conflict,
        };
    }

    return defaultConvert(response.status, updateSkillResultType);
};
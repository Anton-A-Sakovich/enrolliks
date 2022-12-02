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
    success: 'createSkillResult.success',
    validationFailure: 'createSkillResult.validationFailure',
    conflict: 'createSkillResult.conflict',
    badRequest: 'createSkillResult.badRequest',
    serverError: 'createSkillResult.serverError',
    unknownError: 'createSkillResult.unknownError',
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
}
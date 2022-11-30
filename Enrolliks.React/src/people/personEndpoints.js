const { isArray, isObject, repeated, and } = require('../predicates');
const { isPerson } = require('./personUtils');

function convertRemainingStatuses(status, resultTypes) {
    if ((status + "").startsWith("4")) {
        return {
            tag: resultTypes.badRequest,
        };
    }

    if ((status + "").startsWith("5")) {
        return {
            tag: resultTypes.serverError,
        };
    }

    return {
        tag: resultTypes.unknownError,
    };
}

const getPeopleResultType = exports.getPeopleResultType = {
    success: 'getPeopleResultType.success',
    badRequest: 'getPeopleResultType.badRequest',
    serverError: 'getPeopleResultType.serverError',
    unknownError: 'getPeopleResultType.unknownError',
};

exports.getPeople = async (get) => {
    const response = await get('/api/people');

    if (response.status === 200 && and(isArray, repeated(isPerson, 0))(response.data)) {
        return {
            tag: getPeopleResultType.success,
            people: response.data,
        };
    }

    return convertRemainingStatuses(response.status, getPeopleResultType);
};

const createPersonResultType = exports.createPersonResultType = {
    success: 'createPersonResultType.success',
    conflict: 'createPersonResultType.conflict',
    validationFailure: 'createPersonResultType.validationFailure',
    badRequest: 'createPersonResultType.badRequest',
    serverError: 'createPersonResultType.serverError',
    unknownError: 'createPersonResultType.unknownError',
};

exports.createPerson = async (post, personToCreate) => {
    const response = await post('/api/people/create', personToCreate);

    if (response.status === 201 && isPerson(response.data)) {
        return {
            tag: createPersonResultType.success,
            createdPerson: response.data,
        };
    }

    if (response.status === 400 && isObject(response.data)) {
        return {
            tag: createPersonResultType.validationFailure,
            validationErrors: response.data,
        };
    }

    if (response.status === 409) {
        return {
            tag: createPersonResultType.conflict,
        };
    }

    return convertRemainingStatuses(response.status, createPersonResultType);
}

const deletePersonResultType = exports.deletePersonResultType = {
    success: 'deletePersonResultType.success',
    notFound: 'deletePersonResultType.notFound',
    badRequest: 'deletePersonResultType.badRequest',
    serverError: 'deletePersonResultType.serverError',
    unknownError: 'deletePersonResultType.unknownError',
};

exports.deletePerson = async (del, name) => {
    const response = await del(`/api/people/${encodeURIComponent(name)}`);

    if (response.status === 204) {
        return {
            tag: deletePersonResultType.success,
        };
    }

    if (response.status === 404 && response.data === null) {
        return {
            tag: deletePersonResultType.notFound,
        };
    }

    return convertRemainingStatuses(response.status, deletePersonResultType);
};
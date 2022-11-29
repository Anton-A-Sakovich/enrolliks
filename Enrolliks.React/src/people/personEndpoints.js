const getPeopleResultType = exports.getPeopleResultType = {
    success: 0,
    badRequest: 1,
    serverError: 2,
    unknownError: 3,
};

exports.getPeople = async (get) => {
    const response = await get('/api/people');

    if (response.status === 200) {
        return {
            tag: getPeopleResultType.success,
            people: response.data,
        };
    }

    if ((response.status + "").startsWith("4")) {
        return {
            tag: getPeopleResultType.badRequest,
        };
    }

    if ((response.status + "").startsWith("5")) {
        return {
            tag: getPeopleResultType.serverError,
        };
    }

    return {
        tag: getPeopleResultType.unknownError,
    };
};

const createPersonResultType = exports.createPersonResultType = {
    success: 0,
    conflict: 1,
    validationFailure: 2,
    badRequest: 3,
    serverError: 4,
    unknownError: 5,
};

exports.createPerson = async (post, personToCreate) => {
    const response = await post('/api/people/create', personToCreate);

    if (response.status === 201) {
        return {
            tag: createPersonResultType.success,
            createdPerson: response.data,
        };
    }

    if (response.status === 400) {
        return {
            tag: createPersonResultType.validationFailure,
            validationErrors: response.data,
        };
    }

    if (response.status === 404 && typeof (response.data) === 'string') {
        return {
            tag: createPersonResultType.badRequest,
        };
    }

    if (response.status == 409) {
        return {
            tag: createPersonResultType.conflict,
        };
    }

    if (response.status == 500) {
        return {
            tag: createPersonResultType.serverError,
        };
    }

    return {
        tag: createPersonResultType.unknownError,
    };
}

const deletePersonResultType = exports.deletePersonResultType = {
    success: 0,
    notFound: 1,
    badRequest: 2,
    serverError: 3,
    unknownError: 4,
};

exports.deletePerson = async (del, name) => {
    const response = await del(`/api/people/${encodeURIComponent(name)}`);

    switch (response.status) {
        case 204:
            return {
                tag: deletePersonResultType.success,
            };
        
        case 404:
            return {
                tag: deletePersonResultType.notFound,
            };

        case 400:
            return {
                tag: deletePersonResultType.badRequest,
            };

        case 500:
            return {
                tag: deletePersonResultType.serverError,
            };

        default:
            return {
                tag: deletePersonResultType.unknownError,
            };
    }
};
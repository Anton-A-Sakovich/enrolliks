const { isObject, hasProperty, isString, and } = require('../predicates');

exports.validateName = name => {
    if (!name || name.length < 3)
        return 'The name is too short';

    if (name.length > 128)
        return 'The name is too long';

    return '';
};

exports.isPerson = and(isObject,
    hasProperty('name', isString));
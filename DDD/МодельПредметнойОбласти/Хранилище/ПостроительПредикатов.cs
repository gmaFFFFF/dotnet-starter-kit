using System.Linq.Expressions;

namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти;

/// <summary>
///     Обеспечивает динамическое соединение предикатова для запросов
/// </summary>
/// <remarks>
///     Клон PredicateBuilder от Pete Montgomery
///     https://petemontgomery.wordpress.com/2011/02/10/a-universal-predicatebuilder/
/// </remarks>
public static class ПостроительПредикатов {
    /// <summary>
    ///     Создает предикат, всегда возвращающий true
    /// </summary>
    public static Expression<Func<Т, bool>> Истина<Т>() {
        return парам => true;
    }

    /// <summary>
    ///     Создает предикат, всегда возвращающий false
    /// </summary>
    public static Expression<Func<Т, bool>> Ложь<Т>() {
        return парам => false;
    }

    /// <summary>
    ///     Создает предикат из лямбда-выражения
    /// </summary>
    public static Expression<Func<Т, bool>> Создать<Т>(Expression<Func<Т, bool>> предикат) {
        return предикат;
    }

    /// <summary>
    ///     Соединяет первый предикат со вторым, используя логическое "и"
    /// </summary>
    public static Expression<Func<Т, bool>> И<Т>(this Expression<Func<Т, bool>> первый,
        Expression<Func<Т, bool>> второй) {
        return первый.Компоновать(второй, Expression.AndAlso);
    }

    /// <summary>
    ///     Соединяет первый предикат со вторым, используя логическое  "или"
    /// </summary>
    public static Expression<Func<Т, bool>> Или<Т>(this Expression<Func<Т, bool>> первый,
        Expression<Func<Т, bool>> второй) {
        return первый.Компоновать(второй, Expression.OrElse);
    }

    /// <summary>
    ///     Отрицает предикат
    /// </summary>
    public static Expression<Func<Т, bool>> Не<Т>(this Expression<Func<Т, bool>> выражение) {
        var отрицание = Expression.Not(выражение.Body);
        return Expression.Lambda<Func<Т, bool>>(отрицание, выражение.Parameters);
    }

    /// <summary>
    ///     Компонует первое лямбда-выражение со вторым, используя заданную функцию слияния
    /// </summary>
    private static Expression<Т> Компоновать<Т>(this Expression<Т> первое, Expression<Т> второе,
        Func<Expression, Expression, Expression> слить) {
        // попарно соединяет (zip) параметры лямбда-выражения в Dictionary параметров:
        //      ключ - параметры лямбда-выражения "второе";
        //      значение - параметры лямбда-выражения "первое")
        var параметрыСопоставление = первое.Parameters
            .Select((f, i) => new { f, s = второе.Parameters[i] })
            .ToDictionary(p => p.s, p => p.f);

        // заменяет параметры лямбда-выражения "второе" параметрами лямбда-выражения "первое"
        var телоВторого = ПереподключитьПараметры.ЗаменитьПараметры(параметрыСопоставление, второе.Body);

        // создает объединённое лямбда-выражение с параметрами из лямбда-выражения "первое"
        return Expression.Lambda<Т>(слить(первое.Body, телоВторого), первое.Parameters);
    }

    private class ПереподключитьПараметры : ExpressionVisitor {
        private readonly Dictionary<ParameterExpression, ParameterExpression>? _параметрыСопоставление;

        private ПереподключитьПараметры(Dictionary<ParameterExpression, ParameterExpression>? параметрыСопоставление) {
            _параметрыСопоставление =
                параметрыСопоставление ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ЗаменитьПараметры(
            Dictionary<ParameterExpression, ParameterExpression> параметрыСопоставление, Expression выражение) {
            return new ПереподключитьПараметры(параметрыСопоставление).Visit(выражение);
        }

        protected override Expression VisitParameter(ParameterExpression p) {
            if (_параметрыСопоставление!.TryGetValue(p, out var заменитель))
                p = заменитель;

            return base.VisitParameter(p);
        }
    }
}